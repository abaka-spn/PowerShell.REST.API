namespace DynamicPowerShellApi.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    /// The web api element.
    /// </summary>
    public class WebApi : ConfigurationElement
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty("Name", IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["Name"];
            }
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        [ConfigurationProperty("Module", IsKey = false)]
        public string Module
        {
            get
            {
                return (string)this["Module"];
            }
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        [ConfigurationProperty("DiscoveryComands", IsKey = false)]
        public string DiscoveryComands
        {
            get
            {
                return (string)this["DiscoveryComands"];
            }
        }
        /// <summary>
        /// Gets the web methods.
        /// </summary>
        [ConfigurationProperty("WebMethods")]
        public WebMethodCollection WebMethods
        {
            get
            {
                return (WebMethodCollection)this["WebMethods"];
            }
        }

    }
}