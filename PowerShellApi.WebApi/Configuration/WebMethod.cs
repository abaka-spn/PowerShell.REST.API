using System.Linq;

namespace PowerShellRestApi.Configuration
{
    using Newtonsoft.Json.Schema;
    using PowerShellRestApi.PSConfiguration;
    using PowerShellRestApi.WebApi;
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
    public class WebMethod : ConfigurationElement
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
        /// Gets the HttpMethod.
        /// </summary>
        public RestMethod RestMethod
        {
            get
            {
                var i = this.Name.IndexOf("-");
                if (i >= 0)
                {
                    string prefix = this.Name.Substring(0, i);
                    if (Enum.TryParse(prefix, true, out RestMethod restMethod))
                        return restMethod;
                }

                return Constants.DefaultRestMethod;
            }
        }

        /// <summary>
        /// Gets the HttpMethod.
        /// </summary>
        public string Route
        {
            get
            {
                var i = this.Name.IndexOf("-");
                if (i >= 0)
                    return this.Name.Substring(i + 1);

                return this.Name;
            }
        }

        /// <summary>
        /// Gets the PowerShell Command (from module loaded).
        /// </summary>
        [ConfigurationProperty("Command")]
        public string Command
        {
            get
            {
                return (string)this["Command"];
            }
        }

        /// <summary>
        /// Gets the PowerShell path.
        /// </summary>
        [ConfigurationProperty("PowerShellPath")]
        public string PowerShellPath
        {
            get
            {
                return (string)this["PowerShellPath"];
            }
        }

        /// <summary>
        /// Gets the module.
        /// </summary>
        [ConfigurationProperty("Module")]
        public string Module
        {
            get
            {
                return (string)this["Module"];
            }
        }

        /// <summary>
        /// Gets the snap in.
        /// </summary>
        [ConfigurationProperty("Snapin")]
        public string Snapin
        {
            get
            {
                return (string)this["Snapin"];
            }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        [ConfigurationProperty("Parameters")]
        public WebParameterCollection Parameters
        {
            get
            {
                return (WebParameterCollection)this["Parameters"];
            }
        }

        /// <summary>	
        /// 	Run this command as a job? I.e. respond with a Job ID. </summary>
        /// <value>	true if as job, false if not. </value>
        [ConfigurationProperty("AsJob", IsRequired = false, DefaultValue = false)]
        public bool AsJob
        {
            get
            {
                return (bool)this["AsJob"];
            }
        }

        /// <summary>	
        /// Gets if anonymous authentication is allowed</summary>
        /// <value>true if allow, false if not. </value>
        [ConfigurationProperty("AllowAnonymous", IsRequired = false, DefaultValue = false)]
        public bool AllowAnonymous
        {
            get
            {
                return (bool)this["AllowAnonymous"];
            }
        }

        /// <summary>
        /// Gets the roles to grant processing.
        /// </summary>
        [ConfigurationProperty("Roles")]
        public string Roles
        {
            get
            {
                return (string)this["Roles"];
            }
        }

        /// <summary>
        /// Gets the useres to grant processing
        /// </summary>
        [ConfigurationProperty("Users")]
        public string Users
        {
            get
            {
                return (string)this["Users"];
            }
        }

        /// <summary>
        /// Gets the roles parameter name to set when processing
        /// </summary>
        [ConfigurationProperty("ParameterForUserRoles")]
        public string ParameterForUserRoles
        {
            get
            {
                return (string)this["ParameterForUserRoles"] == null ? String.Empty : ((string)this["ParameterForUserRoles"]).Trim();
            }
        }

        /// <summary>
        /// Gets the user parameter name to set when processing
        /// </summary>
        [ConfigurationProperty("ParameterForUserName")]
        public string ParameterForUserName
        {
            get
            {
                return (string)this["ParameterForUserName"] == null ? String.Empty : ((string)this["ParameterForUserName"]).Trim();
            }
        }

        /// <summary>
        /// Gets the claims parameter name to set when processing
        /// </summary>
        [ConfigurationProperty("ParameterForUserClaims")]
        public string ParameterForUserClaims
        {
            get
            {
                return (string)this["ParameterForUserClaims"] == null ? String.Empty : ((string)this["ParameterForUserClaims"]).Trim();
            }
        }

        /*
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

        /// <summary>
        /// Get list of users
        /// </summary>
        private const string USERS = "Users";
        [ConfigurationProperty(USERS)]
        [TypeConverter(typeof(CommaDelimitedStringCollectionConverter))]
        public StringCollection Users
        {
            get
            {
                return (StringCollection)this[USERS];
            }
            set
            {
                base[USERS] = value;
            }
        }

        public Claim[] Claims
        {
            get
            {
                var claims = new List<Claim>();

                if (Users != null)
                    foreach (var u in Users)
                    {
                        claims.Add(new Claim(ClaimTypes.Name, u));
                    }

                if (Roles != null)
                    foreach (var r in Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, r));
                }

                return claims.ToArray();
            }
        }
        */

    }
}