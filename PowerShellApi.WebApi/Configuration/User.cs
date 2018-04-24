using System.Linq;

namespace PowerShellRestApi.Configuration
{
    using Newtonsoft.Json.Schema;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The web method.
    /// </summary>
    public class User : ConfigurationElement
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
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty("ApiKey")]
        public string ApiKey
        {
            get
            {
                return (string)this["ApiKey"];
            }
        }


        /// <summary>
        /// Get list of IpAddresses
        /// </summary>
        private const string IP = "IpAddresses";
        [ConfigurationProperty(IP)]
        [TypeConverter(typeof(CommaDelimitedStringCollectionConverter))]
        public StringCollection IpAddresses
        {
            get
            {
                return (StringCollection)this[IP];
            }
            set
            {
                base[IP] = value;
            }
        }

        /// <summary>
        /// Get list of roles
        /// </summary>
        private const string ROLES = "Roles";
        [ConfigurationProperty(ROLES)]
        [TypeConverter(typeof(CommaDelimitedStringCollectionConverter))]
        public StringCollection Roles
        {
            get
            {
                return (StringCollection)this[ROLES];
            }
            set
            {
                base[ROLES] = value;
            }
        }


        public ClaimsIdentity Identity
        {
            get
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, this.Name));
                foreach (var r in Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, r));
                }
                return new ClaimsIdentity(claims, "ApiKey");
            }
        }

    }
}