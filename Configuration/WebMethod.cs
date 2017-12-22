using System.Linq;

namespace DynamicPowerShellApi.Configuration
{
    using Newtonsoft.Json.Schema;
    using System.Collections.Generic;
    using System.Configuration;

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
        /// Gets the RestMethod.
        /// </summary>
        [ConfigurationProperty("RestMethod", DefaultValue = RestMethod.Unknown)]
        public RestMethod RestMethod
        {
            get
            {
                return (RestMethod)this["RestMethod"];
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
		public ParameterCollection Parameters
		{
			get
			{
				return (ParameterCollection)this["Parameters"];
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
        /// Get PowerShell definition.
        /// </summary>
        public PSCommand ApiCommand { get; set; }

        /*
        /// <summary>
        /// Gets all parameters defined in config file.
        /// </summary>
        public Dictionary<string, PSParameter> GetParametersRequired()
        {
            return ApiCommand.Parameters.Where(x => x.Required)
                             .OrderBy(x => x.Position)
                             .ToDictionary(x => x.Name, x => x);
        }

        /// <summary>
        /// Gets all parameters for specific location.
        /// </summary>
        public Dictionary<string, PSParameter> GetParametersByLocation (RestLocation location)
        {
            return ApiCommand.Parameters.Where(x =>x.Location == RestLocation.Path)
                             .OrderBy(x => x.Position)
                             .ToDictionary(x => x.Name, x => x);
        }


        /*
        //Name of the api on which it depends
        public string ApiName { get; set; }

        //Synopsis of Powershell command (from Powershell Help)
        public string Synopsis { get; set; }

        //Synopsis of Powershell command (from Powershell Help)
        public string Description { get; set; }

        //Name of PowwerShell Command to will be executed
        public string PSCommand { get; set; }

        // Module name of Command
        public string PSModule { get; set; }


        public string GetRoutePath()
        {
            string route = string.Format("{0}/{1}/{2}", "/api", ApiName, Name);

            Parameters.Where(x => x.Location == RestLocation.Path)
                      .OrderBy(x => x.Position)
                      .Select(x => x.Name)
                      .ToList()
                      .ForEach(x => route += "/{" + x + "}");

            return route;
        }

        public string GetQueryString()
        {
            var paz = Parameters.Where(x => x.Location == RestLocation.Query)
                                .OrderBy(x => x.Position)
                                .Select(x => x.Name + "={" + x.TypeName + "}")
                                .ToArray();

            if (paz.Length > 0)
                return "?" + string.Join("&", paz);
            else
                return "";
        }

        public string GetRequestContentType()
        {
            var paz = Parameters.Where(x => x.Location == RestLocation.Body).Select(x => x.JsonType).ToArray();

            if (paz.Length == 0)
                return "text/plain";
            else if (paz.Length == 1 && (paz[0] == JSchemaType.Array || paz[0] == JSchemaType.Object))
                return "text/plain";
            else
                return "application/json";
        }
        */
    }
}