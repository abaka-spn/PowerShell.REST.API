using DynamicPowerShellApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Http;

namespace DynamicPowerShellApi.Configuration
{
    class PSHelpInfo
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
        public PSHelpInfo(CommandInfo cmdInfo, PSObject cmdHelp, WebMethod initialWebMethod, string module, Uri parentUri)
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
        /// <param name="ApiName">Name of api. Use to build route path</param>
        /// <returns></returns>
        public PSCommand GetPSCommand()
        {
            if (_cmdConfig == null || _cmdHelp == null || _cmdInfo == null)
                throw new InvalidOperationException("Command information was not loaded. Use InvokePS() before to load it.");


            FunctionInfo funcInfo = _cmdInfo as FunctionInfo;
            string psVerb = funcInfo?.Verb;

            //UriBuilder uriBuilder = new UriBuilder(_parentUri.ToString());
            //uriBuilder.Path += _cmdConfig.Name;

            var psCmd = new PSCommand
            {
                WebMethodName = _cmdConfig.Name,
                AsJob = _cmdConfig.AsJob,
                Snapin = _cmdConfig.Snapin,
                //Uri = uriBuilder.Uri,
                //Uri = new Uri(string.Format("{0}/{1}",_parentUri.ToString(),_cmdConfig.Name) ,UriKind.Relative),
                Uri = new Uri(_parentUri.ToString() + "/" + _cmdConfig.Route, UriKind.Relative),
                Module = _module,
                ModuleName = _cmdHelp.ModuleName ?? "",
                CommandName = _cmdHelp.Name,
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
                RestMethod = _cmdConfig.RestMethod

            };

            // Not Available cmdInfo.Notes

            if (_cmdHelp.Parameters == null)
            {
                DynamicPowershellApiEvents.Raise.VerboseMessaging(String.Format("Help of command {0} is malformed.", psCmd.CommandName));
                throw new MissingParametersException(String.Format("Help of command {0} is malformed.", psCmd.CommandName));

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
                ParameterAttribute paramAttrib = paramMeta.Attributes.OfType<ParameterAttribute>().FirstOrDefault();

                object defaultValue = ((PSObject)paramHelp.defaultValue)?.BaseObject;
                if (paramHelp.defaultValue != null)
                {
                    if (paramMeta.ParameterType.Equals(typeof(string)))
                        defaultValue = paramHelp.defaultValue.BaseObject;
                    else if (paramMeta.ParameterType.Equals(typeof(int)))
                        defaultValue = int.Parse(defaultValue.ToString());
                }

                Type type = paramMeta.SwitchParameter ? typeof(bool) : paramMeta.ParameterType;

                var psParam = new PSParameter(paramName, type)
                {
                    // from HelpInfo
                    //pi.TypeName = psHelpParam.parameterValue.ToString();
                    DefaultValue = defaultValue,
                    Description = paramHelp.description?[0].Text,
                    Position = int.TryParse(paramHelp.position, out int pos) ? position = pos : ++position,
                    IsSwitch = paramMeta.SwitchParameter,

                    // from CommandInfo
                    Required = paramAttrib.Mandatory,
                    HelpMessage = paramAttrib.HelpMessage,
                    Hidden = paramAttrib.DontShow
                };
                //position = psParam.Position;

                if (paramMeta.Attributes.OfType<ValidateNotNullOrEmptyAttribute>().FirstOrDefault() != null)
                {
                    psParam.AllowEmpty = false;
                    psParam.AllowEmpty = false;
                }
                else
                {
                    psParam.AllowNull = paramMeta.Attributes.OfType<AllowNullAttribute>().FirstOrDefault() != null;
                    psParam.AllowEmpty = (paramMeta.Attributes.OfType<AllowEmptyStringAttribute>().FirstOrDefault() != null) ||
                                         (paramMeta.Attributes.OfType<AllowEmptyCollectionAttribute>().FirstOrDefault() != null);
                }

                paramMeta.Attributes.OfType<ValidateCountAttribute>()
                                .ToList()
                                .ForEach(
                                    x =>
                                    {
                                        psParam.AddValidate(JsonValidate.MinItems, x.MinLength);
                                        psParam.AddValidate(JsonValidate.MaxItems, x.MaxLength);
                                    });

                paramMeta.Attributes.OfType<ValidateLengthAttribute>()
                                .ToList()
                                .ForEach(
                                    x =>
                                    {
                                        psParam.AddValidate(JsonValidate.MinLength, (long)x.MinLength);
                                        psParam.AddValidate(JsonValidate.MaxLength, (long)x.MaxLength);
                                    });

                paramMeta.Attributes.OfType<ValidatePatternAttribute>()
                                .ToList()
                                .ForEach(
                                    x => psParam.AddValidate(JsonValidate.Pattern, x.RegexPattern)
                                );

                paramMeta.Attributes.OfType<ValidateRangeAttribute>()
                                .ToList()
                                .ForEach(
                                    x =>
                                    {
                                        psParam.AddValidate(JsonValidate.Minimum, double.Parse(x.MinRange.ToString()));
                                        psParam.AddValidate(JsonValidate.Maximum, double.Parse(x.MaxRange.ToString()));
                                    });

                paramMeta.Attributes.OfType<ValidateSetAttribute>()
                                .ToList()
                                .ForEach(
                                    x => psParam.AddValidate(JsonValidate.EnumSet, x.ValidValues.ToArray())
                                );


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

                if (psParam.Location == RestLocation.Header && (psParam.Name == "Accept" || psParam.Name == "Content-Type" || psParam.Name == "Authorization"))
                {
                    //TO DO : Write warning

                    psParam.Location = RestLocation.Query;
                }

                //Overload paramters from App.config
                if (paramConfig != null)
                {
                    if (paramConfig.Hidden != null)
                        psParam.Hidden = (bool)paramConfig.Hidden;
                }

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
        public static List<PSHelpInfo> InvokePS(string module, IEnumerable<WebMethod> webMethods, Uri BaseUri)
        {

            var returnValues = new List<PSHelpInfo>();

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
                        DynamicPowershellApiEvents.Raise.ConfigurationError(String.Format("Cannot find module file {0}", module));
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
                            DynamicPowershellApiEvents.Raise.ConfigurationError(String.Format("File {0} not found. The command {1} will be not inventoried", command, webMethod.Name));
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
                            DynamicPowershellApiEvents.Raise.ConfigurationError(String.Format("No information for command {0}.", command));
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
                            returnValues.Add(new PSHelpInfo(psCmdInfo, psCmdHelp, webMethod, module, BaseUri));
                    }
                }

                ps.Dispose();
            }

            return returnValues;
        }

    }
}