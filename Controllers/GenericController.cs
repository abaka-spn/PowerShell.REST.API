using System.IO;
using System.Management.Automation;
using System.Web.Http.Results;
using DynamicPowerShellApi.Jobs;
using DynamicPowerShellApi.Logging;
using DynamicPowerShellApi.Model;
using Newtonsoft.Json;

namespace DynamicPowerShellApi.Controllers
{
    using Configuration;
    using Exceptions;
    using Microsoft.Owin;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;

    /// <summary>
    /// Generic controller for running PowerShell commands.
    /// </summary>
    [Route("api/{*url}")]
    public class GenericController : ApiController
    {
        /// <summary>
        /// The PowerShell runner.
        /// </summary>
        private readonly IRunner _powershellRunner;

        /// <summary>
        /// Crash logger.
        /// </summary>
        private readonly ICrashLogger _crashLogger;

        /// <summary>	The job list provider. </summary>
        private readonly IJobListProvider _jobListProvider;

        /// <summary>
        /// Initialises a new instance of the <see cref="GenericController"/> class.
        /// </summary>
        public GenericController()
        {
        }

        /// <summary>	Initialises a new instance of the <see cref="GenericController"/> class. </summary>
        /// <remarks>	Anthony, 6/1/2015. </remarks>
        /// <exception cref="ArgumentNullException">	Thrown when one or more required arguments are
        /// 											null. </exception>
        /// <param name="powershellRunner">	The PowerShell runner. </param>
        /// <param name="crashLogger">	   	An implementation of a crash logger. </param>
        /// <param name="jobListProvider"> 	The job list provider. </param>
        public GenericController(IRunner powershellRunner, ICrashLogger crashLogger, IJobListProvider jobListProvider)
        {
            _powershellRunner = powershellRunner ?? throw new ArgumentNullException("powershellRunner");
            _crashLogger = crashLogger ?? throw new ArgumentNullException("crashLogger");
            _jobListProvider = jobListProvider ?? throw new ArgumentNullException("jobListProvider");
        }

        /// <summary>
        /// Get a openapi specification
        /// </summary>
        /// <returns>openapi specification</returns>
        [Route("spec")]
        [AllowAnonymous]
        public HttpResponseMessage GetApiSpecification()
        {
            string spec = "";

            var queryStringParams = Request.GetQueryNameValuePairs().ToList();
            var param1 = queryStringParams.FirstOrDefault(kv => kv.Key.Equals("openapi",StringComparison.CurrentCultureIgnoreCase)).Value;

            if (param1 == "2.0")
            {
                spec = OpenApiSpecification.GetSpecAsV2();
            }
            else
            {
                spec = OpenApiSpecification.GetSpecAsV3();
            }            
            return new HttpResponseMessage { Content = new StringContent(spec) };
        }

        /// <summary>
        /// Get a status message
        /// </summary>
        /// <returns>OK always</returns>
        [Route("status")]
        [AllowAnonymous]
        public HttpResponseMessage Status()
        {
            return new HttpResponseMessage { Content = new StringContent("OK") };
        }

        [Route("jobs")]
        public dynamic AllJobStatus()
        {
            dynamic jobs = new
            {
                running = _jobListProvider.GetRunningJobs(),
                completed = _jobListProvider.GetCompletedJobs()
            };

            return jobs;
        }

        /// <summary>	Gets a job. </summary>
        /// <remarks>	Anthony, 6/1/2015. </remarks>
        /// <exception cref="ArgumentOutOfRangeException">	Thrown when one or more arguments are outside
        /// 												the required range. </exception>
        /// <param name="jobId">	Identifier for the job. </param>
        /// <returns>	The job. </returns>
        [Route("job")]
        public dynamic GetJob(string jobId)
        {
            Guid jobGuid;
            if (!Guid.TryParse(jobId, out jobGuid))
                throw new ArgumentOutOfRangeException("jobId is not a valid GUID");

            string jobPath = Path.Combine(WebApiConfiguration.Instance.Jobs.JobStorePath, jobId + ".json");

            if (File.Exists(jobPath))
                using (TextReader reader = new StreamReader(jobPath))
                {
                    dynamic d = JObject.Parse(reader.ReadToEnd());
                    return d;
                }

            return new ErrorResponse
            {
                ActivityId = Guid.NewGuid(),
                LogFile = String.Empty,
                Message = "Cannot find record of job completion"
            };
        }

        /// <summary>
        /// The process request async.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="MalformedUriException">
        /// </exception>
        /// <exception cref="WebApiNotFoundException">
        /// </exception>
        /// <exception cref="WebMethodNotFoundException">
        /// </exception>
        /// <exception cref="MissingParametersException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        [AuthorizeIfEnabled]
        [ProcessRequest]
        public async Task<HttpResponseMessage> ProcessRequestAsync(HttpRequestMessage request = null)
        {


            PSCommand psCommand;
            if (request.Properties.TryGetValue("APP_PSCommand", out object psc) && psc is PSCommand)
                psCommand = (PSCommand)psc;
            else
            {
                DynamicPowershellApiEvents.Raise.VerboseMessaging("Unable to retrieve the PowerShell command");
                throw new PScommandNotFoundException("Unable to retrieve the PowerShell command");
            }

            Guid activityId = Guid.NewGuid();

            bool asJob = psCommand.AsJob;

            //var inParams = new Dictionary<string, object>();
            var inParams = new List<KeyValuePair<string, object>>();
            List<PSParameter> allowedParams;

            #region ----- Body parameters -----
            allowedParams = psCommand.GetParametersByLocation(RestLocation.Body);

            string documentContents = await Request.Content.ReadAsStringAsync();
            documentContents = documentContents.ToString().Trim();

            if (!string.IsNullOrWhiteSpace(documentContents))
            {
                // If only one parameter is defined in parameter file, not name needed 
                if (allowedParams.Count == 1)
                {
                    if (documentContents.StartsWith("["))
                    {
                        JArray tokenArray = JArray.Parse(documentContents);
                        Object value = JsonHelper.Convert(tokenArray);
                        inParams.Add(new KeyValuePair<string, object>(allowedParams.First().Name, value));
                    }
                    else if (documentContents.StartsWith("{"))
                    {
                        JObject obj = JObject.Parse(documentContents);
                        Object value = JsonHelper.Convert(obj);
                        inParams.Add(new KeyValuePair<string, object>(allowedParams.First().Name, value));
                    }
                    else
                    {
                        inParams.Add(new KeyValuePair<string, object>(allowedParams.First().Name, documentContents));
                    }
                }
                else if(allowedParams.Count > 1)
                {
                    if (documentContents.StartsWith("[")) // if more one parameter are defined in config file, array is not allow 
                    {
                        DynamicPowershellApiEvents.Raise.VerboseMessaging(String.Format("Body cannot be a json array."));
                        throw new MissingParametersException("Body cannot be a json array.");
                    }
                    else if(documentContents.StartsWith("{"))  // it's an object. Let's just treat it as an object
                    {
                        JObject obj = JObject.Parse(documentContents);

                        foreach (var detail in obj)
                        {
                            String name = detail.Key;
                            Object value = JsonHelper.Convert(detail.Value);

                            if (allowedParams.FirstOrDefault(x => x.Name.Equals(name,StringComparison.CurrentCultureIgnoreCase)) != null)
                            {
                                inParams.Add(new KeyValuePair<string, object>(name, value));
                            }
                            else
                            {
                                DynamicPowershellApiEvents.Raise.VerboseMessaging(String.Format("Parameter {0} is not allow in body.", name));
                                throw new MissingParametersException(String.Format("Parameter {0} is not allow in body.", name));
                            }
                        }
                    }
                    else
                    {
                        DynamicPowershellApiEvents.Raise.VerboseMessaging(String.Format("Boby must be a json object with {0} properties.",allowedParams.Count));
                        throw new MissingParametersException(String.Format("Boby must be a json object with {0} properties.", allowedParams.Count));
                    }
                }
            }
            #endregion

            #region ----- QueryString parameters -----
            allowedParams = psCommand.GetParametersByLocation(RestLocation.Query);

            foreach (var p in Request.GetQueryNameValuePairs())
            {
                var param = allowedParams.FirstOrDefault(x => x.Name.Equals(p.Key, StringComparison.CurrentCultureIgnoreCase));

                if (param != null)
                {
                    var value = Convert.ChangeType(p.Value, param.Type);

                    inParams.Add(new KeyValuePair<string, object>(param.Name, value));
                }
                else
                {
                    DynamicPowershellApiEvents.Raise.VerboseMessaging(String.Format("Parameter {0} is not allow in QueryString.", p.Key));
                    throw new MissingParametersException(String.Format("Parameter {0} is not allow in QueryString.", p.Key));
                }
            }
            #endregion

            #region ----- URI Path parameters -----
            allowedParams = psCommand.GetParametersByLocation(RestLocation.Path);

            if (Request.RequestUri.Segments.Length - 4 <= allowedParams.Count)
            {
                for (int i = 4; i < Request.RequestUri.Segments.Length; i++)
                {
                    string uriValue = Request.RequestUri.Segments[i].Replace("/", string.Empty);

                    var param = allowedParams.Skip(i - 4).FirstOrDefault();
                    var value = Convert.ChangeType(uriValue, param.Type);

                    inParams.Add(new KeyValuePair<string, object>(param.Name, value));
                }
            }
            else if (Request.RequestUri.Segments.Length - 4 > allowedParams.Count)
            {
                DynamicPowershellApiEvents.Raise.VerboseMessaging(String.Format("Too many parameters in Path."));
                throw new MissingParametersException(String.Format("Too many parameters in Path."));
            }
            #endregion

            #region ----- Header parameters -----
            List<string> allowedParamNames = psCommand.GetParametersByLocation(RestLocation.Header).Select(x => x.Name.ToLower()).ToList();

            /*
            Request.Headers.Where(x => allowedParamNames.Contains(x.Key.ToLower()))
                           .ToList()
                           .ForEach(x => inParams.Add(new KeyValuePair<string, object>(x.Key, x.Value)));
            */

            foreach (var p in Request.Headers)
            {
                var param = allowedParams.FirstOrDefault(x => x.Name.Equals(p.Key, StringComparison.CurrentCultureIgnoreCase));

                if (param != null)
                {
                    var value = Convert.ChangeType(p.Value, param.Type);
                    inParams.Add(new KeyValuePair<string, object>(param.Name, value));
                }
            }

            #endregion

            #region ----- User parameter -----
            if (! string.IsNullOrWhiteSpace(psCommand.ParameterForUser))
            {
                string userName = this.User.Identity != null ? this.User.Identity.Name : "Anonymous";

                inParams.Add(new KeyValuePair<string, object>(psCommand.ParameterForUser, userName));
            }
            #endregion

            #region ----- Roles parameter -----
            if (!string.IsNullOrWhiteSpace(psCommand.ParameterForRoles))
            {
                string[] userRoles = this.User.Identity == null
                                            ? new string[0]
                                            : (this.User.Identity as ClaimsIdentity)
                                                    .Claims
                                                    .Where(c => c.Type == ClaimTypes.Role)
                                                    .Select(c => c.Value)
                                                    .ToArray();

                inParams.Add(new KeyValuePair<string, object>(psCommand.ParameterForRoles, userRoles));
            }
            #endregion

            #region ----- Claims parameter -----
            if (!string.IsNullOrWhiteSpace(psCommand.ParameterForClaims))
            {
                Claim[] userClaims = this.User.Identity == null
                                            ? new Claim[0]
                                            : (this.User.Identity as ClaimsIdentity)
                                                    .Claims
                                                    .ToArray();


                inParams.Add(new KeyValuePair<string, object>(psCommand.ParameterForClaims, userClaims));
            }
            #endregion


            #region ----- Check if all parameters required are found -----
            if (psCommand.GetParametersRequired().Select(x => x.Name).Any(requiedName => inParams.All(q => q.Key != requiedName)))
            {
                DynamicPowershellApiEvents.Raise.VerboseMessaging(String.Format("Cannot find all parameters required."));
                throw new MissingParametersException("Cannot find all parameters required.");
            }
            #endregion

            // We now catch an exception from the runner
            try
            {
                DynamicPowershellApiEvents.Raise.VerboseMessaging(String.Format("Started Executing the runner"));

                if (!asJob)
                {
                    PowershellReturn output =
                        await _powershellRunner.ExecuteAsync(psCommand.Name, psCommand.Snapin, psCommand.Module, inParams, asJob);

                    JToken token = string.IsNullOrWhiteSpace(output.ActualPowerShellData)
                        ? new JObject()
                        : output.ActualPowerShellData.StartsWith("[")
                            ? (JToken)JArray.Parse(output.ActualPowerShellData)
                            : output.ActualPowerShellData.StartsWith("{")
                                ? JObject.Parse(output.ActualPowerShellData)
                                : JObject.Parse("{\"Message\":"+output.ActualPowerShellData+"}");

                    JToken token2 = "";

                    return new HttpResponseMessage
                    {
                        Content = new JsonContent(token)
                    };
                }
                else // run as job.
                {
                    Guid jobId = Guid.NewGuid();
                    string requestedHost = String.Empty;

                    if (Request.Properties.ContainsKey("MS_OwinContext"))
                        requestedHost = ((OwinContext)Request.Properties["MS_OwinContext"]).Request.RemoteIpAddress;

                    _jobListProvider.AddRequestedJob(jobId, requestedHost);

                    // Go off and run this job please sir.
                    var task = Task<bool>.Factory.StartNew(
                        () =>
                        {
                            try
                            {
                                Task<PowershellReturn> goTask =
                                _powershellRunner.ExecuteAsync(
                                        psCommand.Name,
                                        psCommand.Snapin,
                                        psCommand.Module,
                                        inParams,
                                        true);

                                goTask.Wait();
                                var output = goTask.Result;

                                JToken token = output.ActualPowerShellData.StartsWith("[")
                                ? (JToken)JArray.Parse(output.ActualPowerShellData)
                                : JObject.Parse(output.ActualPowerShellData);

                                _jobListProvider.CompleteJob(jobId, output.PowerShellReturnedValidData, String.Empty);

                                string outputPath = Path.Combine(WebApiConfiguration.Instance.Jobs.JobStorePath, jobId + ".json");

                                using (TextWriter writer = File.CreateText(outputPath))
                                {
                                    JsonSerializer serializer = new JsonSerializer
                                    {
                                        Formatting = Formatting.Indented // Make it readable for Ops sake!
                                    };
                                    serializer.Serialize(writer, token);
                                }

                                return true;
                            }
                            catch (PowerShellExecutionException poException)
                            {
                                CrashLogEntry entry = new CrashLogEntry
                                {
                                    Exceptions = poException.Exceptions,
                                    LogTime = poException.LogTime,
                                    RequestAddress = requestedHost,
                                    RequestMethod = psCommand.Name,
                                    RequestUrl = Request.RequestUri.ToString()
                                };
                                entry.SetActivityId(activityId);
                                string logFile = _crashLogger.SaveLog(entry);

                                DynamicPowershellApiEvents.Raise.InvalidPowerShellOutput(poException.Message + " logged to " + logFile);

                                ErrorResponse response =
                                        new ErrorResponse
                                        {
                                            ActivityId = activityId,
                                            LogFile = logFile,
                                            Message = poException.Message
                                        };

                                JToken token = new JObject(response);

                                _jobListProvider.CompleteJob(jobId, false, String.Empty);

                                string outputPath = Path.Combine(WebApiConfiguration.Instance.Jobs.JobStorePath, jobId + ".json");

                                using (TextWriter writer = File.CreateText(outputPath))
                                {
                                    JsonSerializer serializer = new JsonSerializer
                                    {
                                        Formatting = Formatting.Indented // Make it readable for Ops sake!
                                    };
                                    serializer.Serialize(writer, token);
                                }
                                return true;
                            }
                        }
                    );

                    // return the Job ID.
                    return new HttpResponseMessage
                    {
                        Content = new JsonContent(new JValue(jobId))
                    };
                }
            }
            catch(PowerShellClientException ex)
            {
                DynamicPowershellApiEvents.Raise.InvalidPowerShellOutput("["+ ex.Category.ToString()+ "]"+ex.Message);
                switch(ex.Category)
                {
                    case ErrorCategory.PermissionDenied : return Request.CreateErrorResponse(HttpStatusCode.Forbidden, ex.Message);
                    case ErrorCategory.ObjectNotFound   : return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
                    case ErrorCategory.InvalidArgument  : return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "[" + ex.Category.ToString() + "]" + ex.Message);
            }
            catch (PowerShellExecutionException poException)
            {
                string requestedHost = String.Empty;
                if (Request.Properties.ContainsKey("MS_OwinContext"))
                    requestedHost = ((OwinContext)Request.Properties["MS_OwinContext"]).Request.RemoteIpAddress;

                CrashLogEntry entry = new CrashLogEntry
                {
                    Exceptions = poException.Exceptions,
                    LogTime = poException.LogTime,
                    RequestAddress = requestedHost,
                    RequestMethod = psCommand.Name,
                    RequestUrl = Request.RequestUri.ToString()
                };
                entry.SetActivityId(activityId);
                string logFile = _crashLogger.SaveLog(entry);

                DynamicPowershellApiEvents.Raise.InvalidPowerShellOutput(poException.Message + " logged to " + logFile);

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError,
                        new ErrorResponse
                        {
                            ActivityId = activityId,
                            LogFile = logFile,
                            Message = poException.Message
                        }
                        );

                return response;
            }
            catch (Exception ex)
            {
                CrashLogEntry entry = new CrashLogEntry
                {
                    Exceptions = new List<PowerShellException>
                    {
                        new PowerShellException
                        {
                            ErrorMessage = ex.Message,
                            LineNumber = 0,
                            ScriptName = "GenericController.cs",
                            StackTrace = ex.StackTrace
                        }
                    },
                    LogTime = DateTime.Now,
                    RequestAddress = String.Empty, // TODO: Find a way of getting the request host.
                    RequestMethod = psCommand.Name,
                    RequestUrl = Request.RequestUri.ToString()
                };
                entry.SetActivityId(activityId);
                string logFile = _crashLogger.SaveLog(entry);

                DynamicPowershellApiEvents.Raise.UnhandledException(ex.Message + " logged to " + logFile, ex.StackTrace ?? String.Empty);

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.InternalServerError,
                        new ErrorResponse
                        {
                            ActivityId = activityId,
                            LogFile = logFile,
                            Message = ex.Message
                        }
                        );

                return response;
            }
        }
    }
}