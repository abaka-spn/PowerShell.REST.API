using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

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
            { "Remove",  RestMethod.Delete }
        };

        // Public Properties
        public string Key { get { return _initialWebMethod.Name; } }


        // Private Properties
        CommandInfo _cmdInfo { get; set; }

        WebMethod _initialWebMethod { get; set; }

        dynamic _cmdHelp { get; set; }


        public PSHelpInfo(CommandInfo cmdInfo, PSObject cmdHelp, WebMethod initialWebMethod)
        {
            _initialWebMethod = initialWebMethod;

            _cmdInfo = cmdInfo;

            _cmdHelp = cmdHelp; //.Properties;
        }

        public PSCommand GetApiCommandInfo(string ApiName)
        {
            var apiCmd = new PSCommand
            {

                Name = this.Key,
                ApiName = ApiName,
                ModuleName = _cmdHelp.ModuleName ?? "",
                CommandName = _cmdHelp.Name,
                Synopsis = _cmdHelp.Synopsis ?? "",
                Description = _cmdHelp.description == null ? "" : _cmdHelp.description[0].Text,
                OutTypeName = _cmdInfo.OutputType.Select(x => x.Name.ToString()).ToArray(),

                RestMethod = _initialWebMethod.RestMethod != RestMethod.Unknown
                             ? _initialWebMethod.RestMethod
                             : Constants.DefaultRestMethod,
                //else if PsVerbMapping.ContainsKey(CmdInfo.Verb))
                //RestMethod = PsVerbMapping[CmdInfo.Verb]
            };

            if (_initialWebMethod.RestMethod != RestMethod.Unknown)
                apiCmd.RestMethod = _initialWebMethod.RestMethod;
            //else if PsVerbMapping.ContainsKey(CmdInfo.Verb))
            //    RestMethod = CmdInfo.Verb;
            else
                apiCmd.RestMethod = Constants.DefaultRestMethod;


            // Not Available cmdInfo.Notes
            //int i = CmdHelp.Parameters.parameter.Count;// .Where(x => x.Name == "parameters").Count;

            dynamic[] CmdHelpParamters;
            if (_cmdHelp.Parameters.parameter is Array)
                CmdHelpParamters = (dynamic[])_cmdHelp.Parameters.parameter; //.ToArray();
            else
                CmdHelpParamters = new dynamic[] { _cmdHelp.Parameters.parameter };

            foreach (dynamic psHelpParam in CmdHelpParamters)
            {

                // Get Paramter Name
                string ParamName = psHelpParam.name.ToString();
                var initialParam = _initialWebMethod.Parameters.Where(x => x.Name.Equals(ParamName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                // Informations from CommandInfo and HelpInfo
                ParameterMetadata pMeta = _cmdInfo.Parameters[ParamName];
                ParameterAttribute Attrib = pMeta.Attributes.OfType<ParameterAttribute>().FirstOrDefault();


                var pi = new PSParameter(ParamName, pMeta.ParameterType)
                {
                    // from HelpInfo
                    //pi.TypeName = psHelpParam.parameterValue.ToString();
                    DefaultValue = psHelpParam.defaultValue == null ? "" : psHelpParam.defaultValue.ToString(),
                    Description = psHelpParam.description == null ? "" : psHelpParam.description[0].Text,
                    Position = int.Parse(psHelpParam.position),

                    // from CommandInfo
                    //pi.Type = pMeta.ParameterType;
                    Required = Attrib.Mandatory,
                    HelpMessage = Attrib.HelpMessage,
                    Hidden = Attrib.DontShow
                };



                if (pMeta.Attributes.OfType<ValidateNotNullOrEmptyAttribute>().FirstOrDefault() != null)
                {
                    pi.AllowEmpty = false;
                    pi.AllowEmpty = false;
                }
                else
                {
                    pi.AllowNull = pMeta.Attributes.OfType<AllowNullAttribute>().FirstOrDefault() != null;
                    pi.AllowEmpty = (pMeta.Attributes.OfType<AllowEmptyStringAttribute>().FirstOrDefault() != null) ||
                                   (pMeta.Attributes.OfType<AllowEmptyCollectionAttribute>().FirstOrDefault() != null);
                }

                pMeta.Attributes.OfType<ValidateCountAttribute>()
                                .ToList()
                                .ForEach(
                                    x =>
                                    {
                                        pi.AddValidate(PSValidateType.ValidateCount_Min, x.MinLength);
                                        pi.AddValidate(PSValidateType.ValidateCount_Max, x.MaxLength);
                                    });

                pMeta.Attributes.OfType<ValidateLengthAttribute>()
                                .ToList()
                                .ForEach(
                                    x =>
                                    {
                                        pi.AddValidate(PSValidateType.ValidateLength_Min, x.MinLength);
                                        pi.AddValidate(PSValidateType.ValidateLength_Max, x.MaxLength);
                                    });

                pMeta.Attributes.OfType<ValidatePatternAttribute>()
                                .ToList()
                                .ForEach(
                                    x => pi.AddValidate(PSValidateType.ValidatePattern, x.RegexPattern)
                                );

                pMeta.Attributes.OfType<ValidateRangeAttribute>()
                                .ToList()
                                .ForEach(
                                    x =>
                                    {
                                        pi.AddValidate(PSValidateType.ValidateRange_Min, x.MinRange);
                                        pi.AddValidate(PSValidateType.ValidateRange_Max, x.MaxRange);
                                    });

                pMeta.Attributes.OfType<ValidateSetAttribute>()
                                .ToList()
                                .ForEach(
                                    x => pi.AddValidate(PSValidateType.ValidateSet, x.ValidValues.ToArray())
                                );


                //Overload paramters from App.config
                if (initialParam != null)
                {
                    if (initialParam.Hidden != null)
                        pi.Hidden = (bool)initialParam.Hidden;

                    pi.Location = initialParam.Location;
                }


                if (pi.Location == RestLocation.Header && (pi.Name == "Accept" || pi.Name == "Content-Type" || pi.Name == "Authorization"))
                {
                    //TO DO : Avertissement 

                    pi.Location = RestLocation.Query;
                }




                apiCmd.Parameters.Add(pi);
                //apiCmd.Parameters.Add(pi.Name, pi);
            }

            return apiCmd;
        }





        public static List<PSHelpInfo> InvokePS(string module, IEnumerable<WebMethod> webMethods)
        {

            var returnValues = new List<PSHelpInfo>();

            var rsConfig = RunspaceConfiguration.Create();

            var initialSession = InitialSessionState.CreateDefault2();
            if (!String.IsNullOrWhiteSpace(module))
            {
                if (module.EndsWith(".psm1") || module.EndsWith(".psd1"))
                {
                    string strBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    module = File.ReadAllText(Path.Combine(ApiPath.ScriptRepository, module));
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
                            Console.Write(string.Format("Le fichier {0} n'existe pas. La commande {1} ne sera pas inventorié", command, webMethod.Name));
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
                            // DO : Raise Command not found
                            Console.WriteLine(string.Format("Pas d'information a propos de la commande {0} trouvée.", command));
                        }
                        else
                        {
                            psCmdInfo = (CommandInfo)result.FirstOrDefault().BaseObject;
                        }



                        ps.AddCommand("Get-Help").AddParameter("Name", command);
                        result = ps.Invoke();
                        ps.Commands.Clear();

                        if (result.Count == 0)
                        {
                            // DO : Raise Command not found
                            Console.WriteLine(string.Format("Pas d'aide a propos de la commande {0} trouvée.", command));
                        }
                        else
                        {
                            psCmdHelp = result.FirstOrDefault();
                        }

                        if (psCmdInfo != null && psCmdHelp != null)
                            returnValues.Add(new PSHelpInfo(psCmdInfo, psCmdHelp, webMethod));
                    }
                }

                ps.Dispose();
            }

            return returnValues;
        }




    }
}