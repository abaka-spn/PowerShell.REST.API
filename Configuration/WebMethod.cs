using System.Linq;

namespace DynamicPowerShellApi.Configuration
{
    using Newtonsoft.Json.Schema;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Net.Http;
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


                /*
                if (string.IsNullOrWhiteSpace(this["RestMethod"]?.ToString()))
                    return Constants.DefaultRestMethod;
                else
                {
                    return (RestMethod)this["RestMethod"];
                }
                */
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
                    return this.Name.Substring(i+1);

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

        private PSCommand apiCommand;

        /// <summary>
        /// Get PowerShell definition.
        /// </summary>
        public PSCommand GetApiCommand()
        {
            return apiCommand;
        }

        /// <summary>
        /// Get PowerShell definition.
        /// </summary>
        public void SetApiCommand(PSCommand value)
        {
            apiCommand = value;
        }
    }
}