namespace PowerShellRestApi.Configuration
{
    using PowerShellRestApi.PSConfiguration;
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
        /// Gets the Users.
        /// </summary>
        
        [ConfigurationProperty("Users")]
        public UserCollection Users
        {
            get
            {
                return (UserCollection)this["Users"];
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
        /// Gets the JWT authentication.
        /// </summary>
        [ConfigurationProperty("JwtAuthentication", IsRequired = true)]
        public JwtAuthentication JwtAuthentication
        {
            get
            {
                return (JwtAuthentication)this["JwtAuthentication"];
            }
        }

        /// <summary>
        /// Gets the ApiKey authentication.
        /// </summary>
        [ConfigurationProperty("ApiKeyAuthentication", IsRequired = true)]
        public ApiKeyAuthentication ApiKeyAuthentication
        {
            get
            {
                return (ApiKeyAuthentication)this["ApiKeyAuthentication"];
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

    }
}