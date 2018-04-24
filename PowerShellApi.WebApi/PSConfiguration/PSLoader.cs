using PowerShellRestApi.Configuration;
using PowerShellRestApi.WebApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Http;
using System.Security.Claims;

namespace PowerShellRestApi.PSConfiguration
{
    public class PSLoader
    {
        static Dictionary<string, RestMethod> PsVerbMapping = new Dictionary<string, RestMethod>()
        {
            { "New",  RestMethod.Post },
            { "Add",  RestMethod.Post },
            { "Set",  RestMethod.Put },
            { "Get",  RestMethod.Get },
            { "Read",  RestMethod.Get },
            { "Send",  RestMethod.Put },
            { "Write",  RestMethod.Put },
            { "Clear",  RestMethod.Delete },
            { "Remove",  RestMethod.Delete },
            { "Test",  RestMethod.Delete }
        };

        // Public Properties
        //public string WebMethodName { get { return _cmdConfig.Name; } }

        // Private Properties
        WebMethod _cmdConfig { get; set; }

        CommandInfo _cmdInfo { get; set; }

        dynamic _cmdHelp { get; set; }

        string _module { get; set; } = "";

        Uri _parentUri { get; set; }

        /// <summary>
        /// Initialize PSHelpInfo class
        /// </summary>
        /// <param name="cmdInfo">Result of Get-Command cmdlet. Use InvokePS() to build object</param>
        /// <param name="cmdHelp">Result of Get-Help cmdlet. Use InvokePS() to build object</param>
        /// <param name="initialWebMethod">WebMethod instance built from configuration file</param>
        /// <param name="module">Module that must be loaded before running the command</param>
        public PSLoader(CommandInfo cmdInfo, PSObject cmdHelp, WebMethod initialWebMethod, string module, Uri parentUri)
        {
            _cmdConfig = initialWebMethod;

            _cmdInfo = cmdInfo;

            _cmdHelp = cmdHelp; //.Properties;

            _module = module;

            _parentUri = parentUri;
        }

        /// <summary>
        /// Get PSCommand object from information loaded by InvokePS()
        /// </summary>
        /// <returns></returns>
        public PSCommand GetPSCommand()
        {
            if (_cmdConfig == null || _cmdHelp == null || _cmdInfo == null)
                throw new InvalidOperationException("Command information was not loaded. Use InvokePS() before to load it.");


            FunctionInfo funcInfo = _cmdInfo as FunctionInfo;
            string psVerb = funcInfo?.Verb;

            //UriBuilder uriBuilder = new UriBuilder(_parentUri.ToString());
            //uriBuilder.Path += _cmdConfig.Name;

            var psCmd = new PSCommand(_cmdHelp.Name)
            {
                WebMethodName = _cmdConfig.Name,
                AsJob = _cmdConfig.AsJob,
                Snapin = _cmdConfig.Snapin,
                //Uri = uriBuilder.Uri,
                //Uri = new Uri(string.Format("{0}/{1}",_parentUri.ToString(),_cmdConfig.Name) ,UriKind.Relative),
                Uri = new Uri(_parentUri.ToString() + "/" + _cmdConfig.Route, UriKind.Relative),
                Module = _module,
                ModuleName = _cmdHelp.ModuleName ?? "",
                //Name = _cmdHelp.Name,
                Synopsis = _cmdHelp.Synopsis ?? "",
                Description = _cmdHelp.description == null ? "" : _cmdHelp.description[0].Text,
                OutTypeName = _cmdInfo.OutputType.Select(x => x.Name.ToString()).ToArray(),

                /*
                RestMethod = _cmdConfig.RestMethod != null
                             ? (RestMethod)_cmdConfig.RestMethod
                             : psVerb != null && PsVerbMapping.ContainsKey(psVerb)
                                ? PsVerbMapping[psVerb]
                                : Constants.DefaultRestMethod
                */
                RestMethod = _cmdConfig.RestMethod,
                Roles = _cmdConfig.Roles,
                Users = _cmdConfig.Users,
                AllowAnonymous = _cmdConfig.AllowAnonymous
            };

            // Not Available cmdInfo.Notes

            if (_cmdHelp.Parameters == null)
            {
                PowerShellRestApiEvents.Raise.VerboseMessaging(String.Format("Help of command {0} is malformed.", psCmd.Name));
                throw new MissingParametersException(String.Format("Help of command {0} is malformed.", psCmd.Name));

            }

            dynamic[] psHelpParamters = _cmdHelp.Parameters.parameter is Array
                                        ? (dynamic[])_cmdHelp.Parameters.parameter
                                        : new dynamic[] { _cmdHelp.Parameters.parameter };

            int position = 0;
            foreach (dynamic paramHelp in psHelpParamters)
            {
                // Get Paramter Name
                string paramName = paramHelp.name.ToString();
                var paramConfig = _cmdConfig.Parameters.Where(x => x.Name.Equals(paramName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                // Informations from CommandInfo and HelpInfo
                ParameterMetadata paramMeta = _cmdInfo.Parameters[paramName];

                object defaultValue = ((PSObject)paramHelp.defaultValue)?.BaseObject;
                if (paramHelp.defaultValue != null)
                {
                    if (paramMeta.ParameterType.Equals(typeof(string)))
                        defaultValue = paramHelp.defaultValue.BaseObject;
                    else if (paramMeta.ParameterType.Equals(typeof(int)))
                        defaultValue = int.Parse(defaultValue.ToString());
                }

                Type paramType = paramMeta.SwitchParameter ? typeof(bool) : paramMeta.ParameterType;

                var psParam = new PSParameter(paramName, paramType)
                {
                    // from HelpInfo
                    DefaultValue = defaultValue,
                    Description = paramHelp.description?[0].Text,
                    Position = int.TryParse(paramHelp.position, out int pos) ? position = pos : ++position,
                    IsSwitch = paramMeta.SwitchParameter,
                };

                // Add validation attribute from CommandInfo
                psParam.AddValidate(paramMeta.Attributes.ToArray());

                // Set parameter name to ParameterForUser if it matches the name defined in config file
                if (paramName.Equals(_cmdConfig.ParameterForUserName, StringComparison.InvariantCultureIgnoreCase) &&
                    paramType == typeof(String))
                {
                    psCmd.ParameterForUser = paramName;
                    psParam.Hidden = true;
                }
                // Set parameter name to ParameterForRoles if it matches the name defined in config file
                else if (paramName.Equals(_cmdConfig.ParameterForUserRoles, StringComparison.InvariantCultureIgnoreCase) &&
                    paramType == typeof(String[]))
                {
                    psCmd.ParameterForRoles = paramName;
                    psParam.Hidden = true;
                }
                // Set parameter name to ParameterForClaims if it matches the name defined in config file
                else if (paramName.Equals(_cmdConfig.ParameterForUserClaims, StringComparison.InvariantCultureIgnoreCase) &&
                    paramType == typeof(Claim[]))
                {
                    psCmd.ParameterForClaims = paramName;
                    psParam.Hidden = true;
                }
                // Set parameter name to ParameterForClaims if it is same type and ParameterForClaims is not defined in config file
                else if (string.IsNullOrWhiteSpace(_cmdConfig.ParameterForUserClaims) &&
                    paramType == typeof(Claim[]))
                {
                    psCmd.ParameterForClaims = paramName;
                    psParam.Hidden = true;
                }
                //Override this parameter if it set in from App.config
                else if (paramConfig?.Hidden != null)
                {
                    psParam.Hidden = (bool)paramConfig.Hidden;
                }

                // Static file from configuration file
                if (!string.IsNullOrWhiteSpace(paramConfig?.Value))
                {
                    psParam.Location = RestLocation.ConfigFile;
                    psParam.Value = paramConfig.Value;
                }
                else
                {
                    // Parameter location
                    if (paramConfig?.Location == null)
                    {
                        if (psCmd.RestMethod == RestMethod.Get)
                            psParam.Location = RestLocation.Query;
                        else
                            psParam.Location = RestLocation.Body;
                    }
                    else
                    {
                        psParam.Location = (RestLocation)paramConfig.Location;
                    }
                }

                if (psParam.Location == RestLocation.Header && (psParam.Name == "Accept" || psParam.Name == "Content-Type" || psParam.Name == "Authorization"))
                {
                    //TO DO : Write warning

                    psParam.Location = RestLocation.Query;
                }

                if (!psParam.Hidden)
                    psParam.GenerateModel();

                // Add new parameter in collection
                psCmd.Parameters.Add(psParam);
            }
            
            return psCmd;
        }


        /// <summary>
        /// Invoke PowerShell commands (Get-Command and Get-Help) to retrieve information about the input PowerShell command or module
        /// </summary>
        /// <param name="module">Module that must be loaded before running the command</param>
        /// <param name="webMethods">List of WebMethod instance built from configuration file</param>
        /// <returns></returns>
        public static List<PSLoader> InvokePS(string module, IEnumerable<WebMethod> webMethods, Uri BaseUri)
        {

            var returnValues = new List<PSLoader>();

            var rsConfig = RunspaceConfiguration.Create();

            var initialSession = InitialSessionState.CreateDefault2();
            if (!String.IsNullOrWhiteSpace(module))
            {
                if (module.EndsWith(".psm1") || module.EndsWith(".psd1"))
                {
                    module = Path.Combine(ApiPath.ScriptRepository, module);

                    if (! File.Exists(module))
                    {
                        Console.WriteLine(String.Format("Cannot find module file {0}", module));
                        PowerShellRestApiEvents.Raise.ConfigurationError(String.Format("Cannot find module file {0}", module));
                    }
                }

                initialSession.ImportPSModule(new[] { module });
            }



            using (PowerShell ps = PowerShell.Create())
            {
                RunspacePool _runspacePool = RunspaceFactory.CreateRunspacePool(initialSession);
                _runspacePool.Open();
                ps.RunspacePool = _runspacePool;

                //for (int i=0; i < commands.Length; i++)
                foreach (WebMethod webMethod in webMethods)
                {


                    string command = "";

                    if (!string.IsNullOrWhiteSpace(webMethod.Command))
                    {
                        command = webMethod.Command;
                    }
                    else
                    {
                        command = Path.Combine(ApiPath.ScriptRepository, webMethod.PowerShellPath);

                        //TO DO tester le présence du fichier
                        if (!File.Exists(command))
                        {
                            Console.WriteLine(String.Format("File {0} not found. The command {1} will be not inventoried", command, webMethod.Name));
                            PowerShellRestApiEvents.Raise.ConfigurationError(String.Format("File {0} not found. The command {1} will be not inventoried", command, webMethod.Name));
                        }
                    }

                    if (command != "")
                    {
                        CommandInfo psCmdInfo = null;
                        PSObject psCmdHelp = null;


                        ps.AddCommand("Get-Command").AddParameter("Name", command);
                        Collection<PSObject> result = ps.Invoke();
                        ps.Commands.Clear();

                        if (result.Count == 0)
                        {
                            Console.WriteLine(String.Format("No information for command {0}.", command));
                            PowerShellRestApiEvents.Raise.ConfigurationError(String.Format("No information for command {0}.", command));
                        }
                        else
                        {
                            psCmdInfo = (CommandInfo)result.FirstOrDefault().BaseObject;
                        }



                        ps.AddCommand("Get-Help").AddParameter("Name", command).AddParameter("Full");
                        result = ps.Invoke();
                        ps.Commands.Clear();

                        if (result.Count == 0)
                        {
                            // DO : Raise Command not found
                            Console.WriteLine(string.Format("No PowerShell help for command {0}.", command));
                        }
                        else
                        {
                            psCmdHelp = result.FirstOrDefault();
                        }

                        if (psCmdInfo != null && psCmdHelp != null)
                            returnValues.Add(new PSLoader(psCmdInfo, psCmdHelp, webMethod, module, BaseUri));
                    }
                }

                ps.Dispose();
            }

            return returnValues;
        }


        public static void LoadPSCommands()
        {
            // Check if script files or modules files exist
            List<string> files = new List<string>();

            WebApiConfiguration.Instance.Apis
                               .Select(x => x.Module)
                               .Where(x => x.EndsWith(".psm1") ||
                                           x.EndsWith(".psd1"))
                               .ToList()
                               .ForEach(x => files.Add(x));

            WebApiConfiguration.Instance.Apis
                               .SelectMany(x => x.WebMethods)
                               .Select(x => x.Module)
                               .Where(x => x.EndsWith(".psm1") ||
                                           x.EndsWith(".psd1"))
                               .ToList()
                               .ForEach(x => files.Add(x));

            WebApiConfiguration.Instance.Apis
                               .SelectMany(x => x.WebMethods)
                               .Select(x => x.PowerShellPath)
                               .Where(x => x.EndsWith(".ps1"))
                               .Distinct()
                               .ToList()
                               .ForEach(x => files.Add(x));

            foreach (string file in files.Distinct())
            {
                var path = Path.Combine(ApiPath.ScriptRepository, file);

                if (!File.Exists(path))
                {
                    Console.WriteLine(String.Format("Cannot find PowerShell file {0}", path));
                    PowerShellRestApiEvents.Raise.ConfigurationError(String.Format("Cannot find PowerShell file {0}", path));
                }
            }


            foreach (Configuration.WebApi api in WebApiConfiguration.Instance.Apis)
            {
                bool isModuleMode = !String.IsNullOrWhiteSpace(api.Module);

                Uri BaseUri = new Uri("/api/" + api.Name, UriKind.Relative);

                List<PSLoader> psHelpInfo;

                if (isModuleMode)
                {
                    PSLoader.InvokePS(api.Module, api.WebMethods, BaseUri)
                            .ForEach(x => Routes.Add(x.GetPSCommand()));
                }
                else
                {
                    psHelpInfo = new List<PSLoader>();

                    foreach (WebMethod webMethod in api.WebMethods)
                    {
                        PSLoader.InvokePS(webMethod.Module, new[] { webMethod }, BaseUri)
                                .ForEach(x => Routes.Add(x.GetPSCommand()));
                    }
                }

            }



        }

    }
}