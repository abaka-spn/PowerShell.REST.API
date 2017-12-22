namespace DynamicPowerShellApi.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

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
        [ConfigurationProperty("Version", IsRequired = false)]
        public string Version
        {
            get
            {
                return (string)this["Version"];
            }
        }


        public static void LoadPSCommands()
        {
            foreach (WebApi api in WebApiConfiguration.Instance.Apis)
            {
                bool isModuleMode = !String.IsNullOrWhiteSpace(api.Module);


                List<PSHelpInfo> psHelpInfo;

                if (isModuleMode)
                {
                    psHelpInfo = PSHelpInfo.InvokePS(api.Module, api.WebMethods);
                }
                else
                {
                    psHelpInfo = new List<PSHelpInfo>();

                    foreach (WebMethod webMethod in api.WebMethods)
                    {
                        PSHelpInfo.InvokePS(api.Module, new[] { webMethod })
                                  .ForEach(x => psHelpInfo.Add(x));
                    }
                }

                foreach (var onePsHelpInfo in psHelpInfo)
                {
                    //Fill and Add Command informations to the instance
                    api.WebMethods[onePsHelpInfo.Key].ApiCommand = onePsHelpInfo.GetApiCommandInfo(api.Name);
                }
            }



        }
    }
}