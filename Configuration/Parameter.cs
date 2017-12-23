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
        [ConfigurationProperty("Location", DefaultValue = RestLocation.Body)]
        public RestLocation Location
        {
            get
            {
                return (RestLocation)this["Location"];
            }
        }


        public bool IsDefinedInPSCommand { get; set; } = false;

        /*
        bool _required = false;
        public bool Required
        {
            get
            {
                return _required;
            }
            set
            {
                _required = value;

                //TO DO : Add in JSchema
            }
        }

        private static readonly Dictionary<Type, JSchemaType> JSchemaTypeMap =
            new Dictionary<Type, JSchemaType>
            {
                { typeof(char), JSchemaType.String },
                { typeof(char?), JSchemaType.String },
                { typeof(bool), JSchemaType.Boolean },
                { typeof(bool?), JSchemaType.Boolean},
                { typeof(sbyte), JSchemaType.Integer },
                { typeof(sbyte?), JSchemaType.Integer },
                { typeof(short), JSchemaType.Integer },
                { typeof(short?), JSchemaType.Integer },
                { typeof(ushort), JSchemaType.Integer },
                { typeof(ushort?), JSchemaType.Integer },
                { typeof(int), JSchemaType.Integer },
                { typeof(int?), JSchemaType.Integer },
                { typeof(byte), JSchemaType.Integer },
                { typeof(byte?), JSchemaType.Integer },
                { typeof(uint), JSchemaType.Integer },
                { typeof(uint?), JSchemaType.Integer },
                { typeof(long), JSchemaType.Integer},
                { typeof(long?), JSchemaType.Integer },
                { typeof(ulong), JSchemaType.Integer },
                { typeof(ulong?), JSchemaType.Integer },
                { typeof(float), JSchemaType.Number },
                { typeof(float?), JSchemaType.Number },
                { typeof(double), JSchemaType.Number },
                { typeof(double?), JSchemaType.Number },
                { typeof(DateTime), JSchemaType.String },
                { typeof(DateTime?), JSchemaType.String },
#if HAVE_DATE_TIME_OFFSET
                { typeof(DateTimeOffset), JSchemaType.String },
                { typeof(DateTimeOffset?), JSchemaType.String },
#endif
                { typeof(decimal), JSchemaType.Number },
                { typeof(decimal?), JSchemaType.Number },
                { typeof(Guid), JSchemaType.String },
                { typeof(Guid?), JSchemaType.String },
                { typeof(TimeSpan), JSchemaType.String },
                { typeof(TimeSpan?), JSchemaType.String },
#if HAVE_BIG_INTEGER
                { typeof(BigInteger), JSchemaType.Integer },
                { typeof(BigInteger?), JSchemaType.Integer },
#endif
                { typeof(Uri), JSchemaType.String },
                { typeof(string), JSchemaType.String },
                { typeof(byte[]), JSchemaType.String },
#if HAVE_ADO_NET
                { typeof(DBNull), JSchemaType.DBNull }
#endif
            };



        public int Position { get; set; }

        JSchema _jsonSchema = new JSchema();

        Type _type;
        public Type Type
        {
            get { return _type; }
            set
            {
                _type = value;

                JSchemaType schemaType = JSchemaType.None;

                if (JSchemaTypeMap.ContainsKey(_type))
                {
                    _jsonSchema.Type = schemaType | JSchemaTypeMap[_type];
                }
                else
                {
                    _jsonSchema.Type = schemaType | JSchemaType.String | JSchemaType.Boolean | JSchemaType.Integer | JSchemaType.Number | JSchemaType.Object | JSchemaType.Array;
                }
            }
        }

        public string TypeName { get { return _type.Name; } }

        public void AddValidate(string name, object validate)
        {
            if (name.Equals("minimum", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.Minimum = (double)validate; // -> ValidateRange
            if (name.Equals("maximum", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.Maximum = (double)validate; // -> ValidateRange
            if (name.Equals("maxLength", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.MaximumLength = (int)validate; // -> ValidateLength
            if (name.Equals("minLength", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.MinimumLength = (int)validate; // -> ValidateLength
            if (name.Equals("pattern", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.Pattern = (string)validate;      // -> ValidatePattern
            if (name.Equals("maxItems", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.MaximumItems = (int)validate;   // -> ValidateCount
            if (name.Equals("minItems", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.MinimumItems = (int)validate;   // -> ValidateCount

            if (name.Equals("enum", StringComparison.CurrentCultureIgnoreCase))
            {
                JArray.FromObject(validate).ToList().ForEach(x => _jsonSchema.Enum.Add(x)); // -> ValidateSet
            }

            /* ***** In PowerShell is always false *****
            if (name.Equals("exclusiveMinimum",StringComparison.CurrentCultureIgnoreCase))
                _JsonSchema.ExclusiveMinimum = (bool)validate; // -> N/A
            if (name.Equals("exclusiveMaximum",StringComparison.CurrentCultureIgnoreCase))
                _JsonSchema.ExclusiveMaximum = (bool)validate; // -> N/A
            *

            /* ***** Not available with PowerShell Validate Attributes *****
            if (name.Equals("multipleOf",StringComparison.CurrentCultureIgnoreCase))
                _JsonSchema.MultipleOf = (double)validate;
            if (name.Equals("uniqueItems",StringComparison.CurrentCultureIgnoreCase))
                _JsonSchema.UniqueItems = (bool)validate; // -> N/A
            if (name.Equals("maxProperties",StringComparison.CurrentCultureIgnoreCase))
                _JsonSchema.MaxProperties = (int)validate; // -> N/A
            if (name.Equals("minProperties",StringComparison.CurrentCultureIgnoreCase))
                _JsonSchema.MinProperties = (int)validate; // -> N/A
                *

        }
   
        public void RemoveValidate(string name)
        {
            if (name.Equals("minimum", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.Minimum = null;
            if (name.Equals("maximum", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.Maximum = null;
            if (name.Equals("maxLength", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.MaximumLength = null;
            if (name.Equals("minLength", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.MinimumLength = null;
            if (name.Equals("pattern", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.Pattern = null;
            if (name.Equals("maxItems", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.MaximumItems = null;
            if (name.Equals("minItems", StringComparison.CurrentCultureIgnoreCase))
                _jsonSchema.MinimumItems = null;

        }
        */
 
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