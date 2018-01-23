namespace DynamicPowerShellApi.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// The web api configuration.
    /// </summary>
    public class WebApiConfiguration : ConfigurationSection
    {
        /// <summary>
        /// The singleton object of the web API configuration.
        /// </summary>
        private static readonly Lazy<WebApiConfiguration> LazyConfiguration =
            new Lazy<WebApiConfiguration>(() => (WebApiConfiguration)ConfigurationManager.GetSection("WebApiConfiguration"));

        /// <summary>
        /// Prevents a default instance of the <see cref="WebApiConfiguration"/> class from being created.
        /// </summary>
        private WebApiConfiguration()
        {
        }

        /// <summary>
        /// Gets the singleton instance of <see cref="WebApiConfiguration"/>.
        /// </summary>
        public static WebApiConfiguration Instance
        {
            get
            {
                return LazyConfiguration.Value;
            }
        }

        /// <summary>
        /// Gets the APIs.
        /// </summary>
        [ConfigurationProperty("Apis")]
        public WebApiCollection Apis
        {
            get
            {
                return (WebApiCollection)this["Apis"];
            }
        }

        /// <summary>
        /// Gets the host address.
        /// </summary>
        [ConfigurationProperty("HostAddress", IsRequired = true)]
        public Uri HostAddress
        {
            get
            {
                return (Uri)this["HostAddress"];
            }
        }

        /// <summary>
        /// Gets the authentication.
        /// </summary>
        [ConfigurationProperty("Authentication", IsRequired = true)]
        public Authentication Authentication
        {
            get
            {
                return (Authentication)this["Authentication"];
            }
        }

        /// <summary>	Gets the jobs configuration object. </summary>
        [ConfigurationProperty("Jobs", IsRequired = true)]
        public JobStoreConfiguration Jobs
        {
            get
            {
                return (JobStoreConfiguration)this["Jobs"];
            }
        }

        /// <summary>
        /// Gets the singleton instance of <see cref="WebApiConfiguration"/>.
        /// </summary>
        [ConfigurationProperty("Title", IsRequired = false)]
        public string Title
        {
            get
            {
                return (string)this["Title"];
            }
        }

        /// <summary>
        /// Gets the singleton instance of <see cref="WebApiConfiguration"/>.
        /// </summary>
        [ConfigurationProperty("Version", IsRequired = false, DefaultValue = "0.0.0.0")]
        [RegexStringValidator(@"^\d+\.\d+\.\d+\.\d+$*")]
        public string Version
        {
            get
            {
                return (string)this["Version"];
            }
        }


        public static Dictionary<RestMethod, Dictionary<string, PSCommand>> Routes = new Dictionary<RestMethod, Dictionary<string, PSCommand>>
        {
            { RestMethod.Delete, new Dictionary<string, PSCommand>() },
            { RestMethod.Get, new Dictionary<string, PSCommand>() },
            { RestMethod.Patch, new Dictionary<string, PSCommand>() },
            { RestMethod.Post, new Dictionary<string, PSCommand>() },
            { RestMethod.Put, new Dictionary<string, PSCommand>() }
        };

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
                    DynamicPowershellApiEvents.Raise.ConfigurationError(String.Format("Cannot find PowerShell file {0}", path));
                }
            }


            foreach (WebApi api in WebApiConfiguration.Instance.Apis)
            {
                bool isModuleMode = !String.IsNullOrWhiteSpace(api.Module);
                
                Uri BaseUri = new Uri("/api/" + api.Name, UriKind.Relative);

                List<PSHelpInfo> psHelpInfo;

                if (isModuleMode)
                {
                    psHelpInfo = PSHelpInfo.InvokePS(api.Module, api.WebMethods, BaseUri);
                }
                else
                {
                    psHelpInfo = new List<PSHelpInfo>();

                    foreach (WebMethod webMethod in api.WebMethods)
                    {
                        PSHelpInfo.InvokePS(webMethod.Module, new[] { webMethod }, BaseUri)
                                  .ForEach(x => psHelpInfo.Add(x));
                    }
                }

                //if (api.DiscoveryComands != "")

                foreach (var onePsHelpInfo in psHelpInfo)
                {
                    //Fill and Add Command informations to the instance
                    PSCommand psCommand = onePsHelpInfo.GetPSCommand();

                    api.WebMethods[psCommand.WebMethodName].ApiCommand = psCommand;

                    Routes[psCommand.RestMethod][psCommand.GetRoutePath().ToLower()] = psCommand;

                }
            }



        }
    }
}