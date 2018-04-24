using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellRestApi.Configuration
{
    public class ApiKeyAuthentication : ConfigurationElement
    {
        /// <summary>
        /// Gets the HeaderKey. see https://github.com/jamesharling/Microsoft.Owin.Security.ApiKey
        /// </summary>
        [ConfigurationProperty("HeaderKey", IsRequired = false)]
        public string HeaderKey
        {
            get
            {
                return (string)this["HeaderKey"];
            }
        }

        /// <summary>
        /// Gets the Header. see https://github.com/jamesharling/Microsoft.Owin.Security.ApiKey
        /// </summary>
        [ConfigurationProperty("Header", IsRequired = false)]
        public string Header
        {
            get
            {
                return (string)this["Header"];
            }
        }

        /// <summary>
        /// Enable authentication.
        /// </summary>
        [ConfigurationProperty("Enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get
            {
                return (bool)this["Enabled"];
            }
        }

    }
}
