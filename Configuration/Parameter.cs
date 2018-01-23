namespace DynamicPowerShellApi.Configuration
{
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    /// <summary>
    /// The parameter element.
    /// </summary>
    public class Parameter : ConfigurationElement
	{
        //internal WebMethod ParentWebMethod { get; set; }

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
		/// Gets the parameter type.
		/// </summary>
		[ConfigurationProperty("Type")]
		public string ParamType
		{
			get
			{
				return (string)this["Type"];
			}
		}

        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        [ConfigurationProperty("Hidden")]
        public bool? Hidden
        {
            get
            {
                return (bool?)this["Hidden"];
            }
        }

        /// <summary>
        /// Gets the parameter type.
        /// </summary>
        [ConfigurationProperty("Location")]
        public RestLocation? Location
        {
            get
            {
                return (RestLocation?)this["Location"];
            }
        }


        public bool IsDefinedInPSCommand { get; set; } = false;

        /// <summary>
		/// Determines whether the parameter is optional.
		/// </summary>
		[ConfigurationProperty("IsOptional", DefaultValue = false)]
		public bool IsOptional
		{
			get
			{
				return (bool)this["IsOptional"];
			}
		}

		/// <summary>
		/// Determines whether the parameter is required (configured via <see cref="IsOptional"/>).
		/// </summary>
		public bool IsRequired => !IsOptional;
	}
}