using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;


namespace DynamicPowerShellApi.Configuration
{

    public class PSParameter
    {

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

        private static readonly Dictionary<Type, Func<object, IOpenApiAny>> ConvertObjectToOpenApi = 
            new Dictionary<Type, Func<object, IOpenApiAny>>
            {
                {typeof(int), (x) => new OpenApiInteger((int)x) },
                {typeof(string), (x) => new OpenApiString((string)x) },
                {typeof(byte[]), (x) => new OpenApiBinary((byte[])x) },
                {typeof(bool), (x) => new OpenApiBoolean((bool)x) },
                {typeof(byte), (x) => new OpenApiByte((byte)x) },
                {typeof(DateTime), (x) => new OpenApiDate((DateTime)x) },
                {typeof(double), (x) => new OpenApiDouble((double)x) },
                {typeof(float), (x) => new OpenApiFloat((float)x) },
                {typeof(long), (x) => new OpenApiLong((long)x) }
                //{typeof(string), (x) => new OpenApiPassword((string)x) },
                //{typeof(object), (x) => new OpenApiObject((object)x) },
                //{typeof(null), (x) => new OpenApiNull((null)x) },
            };

        JSchema _jsonSchema = new JSchema();
        Type _type;

        /// <summary>
        /// Name of parameter
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// .Net type of parameter
        /// </summary>
        public Type Type
        {
            get { return _type; }
            set
            {
                _type = value;

                //JSchemaType schemaType = JSchemaType.None;
                if (_type.IsArray)
                {
                    _jsonSchema.Type = JSchemaType.Array;

                    var elementType = _type.GetElementType();
                    if (JSchemaTypeMap.ContainsKey(elementType))
                    {
                        _jsonSchema.Items.Add(new JSchema() { Type = JSchemaTypeMap[elementType] });
                    }
                    else
                    {
                        _jsonSchema.Items.Add(new JSchema() { Type = JSchemaType.Object });
                    }
                }
                else if (JSchemaTypeMap.ContainsKey(_type))
                {
                    _jsonSchema.Type = JSchemaTypeMap[_type];
                }
                else
                {
                    _jsonSchema.Type = JSchemaType.Object;
                }
            }
        }

        /// <summary>
        /// Name of .Net type
        /// </summary>
        public string TypeName { get { return _type.Name; } }

        /* JSON Schema Data Types 
           https://github.com/OAI/OpenAPI-Specification/blob/master/versions/3.0.0.md#dataTypeFormat
        Common Name type    format  Comments
        integer     integer int32   signed 32 bits
        long        integer int64   signed 64 bits
        float       number
        float       double
        number      double
        string      string
        byte        string  byte    base64 encoded characters
        binary      string  binary  any sequence of octets
        boolean     boolean 
        date        string  date    As defined by full-date - RFC3339
        dateTime    string  date-time   As defined by date-time - RFC3339
        password    string  password    A hint to UIs to obscure input.
         */

        /// <summary>
        /// Json type name
        /// </summary>
        public JSchemaType JsonType
        {
            get
            {
                return (JSchemaType)_jsonSchema.Type;
            }
        }

        /// <summary>
        /// Position of parameter in URL for path location
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Help message for paramter [Parameter(HelpMessage="bla bla ...")]
        /// </summary>
        public string HelpMessage { get; set; } = "";

        /// <summary>
        /// Define if this parameter is required. (Add [Parameter(Mandatory=$true)] in PowerShell script)
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Define if a hidden paramter (Add [Parameter(DontShow)] in PowerShell script)
        /// </summary>
        public bool Hidden { get; set; } = false;

        /// <summary>
        /// Define if is a [switch] parameter in PowerShell Script
        /// </summary>
        public bool IsSwitch { get; set; } = false;

        public bool AllowNull { get; set; } = true;

        public bool AllowEmpty { get; set; } = true;

        public object DefaultValue { get; set; }

        /// <summary>
        /// Description of Help command 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Location of parameter in rest query (body, query, path, ...)
        /// </summary>
        public RestLocation Location { get; set; } = Constants.DefaultParameterLocation;

        public Dictionary<JsonValidate, object> Validates = new Dictionary<JsonValidate, object>();

        public void AddValidate(JsonValidate name, object validate)
        {
            Validates.Add(name, validate);

            switch (name)
            {
                case JsonValidate.Minimum:
                    _jsonSchema.Minimum = validate as double?; // -> ValidateRange
                    break;
                case JsonValidate.Maximum:
                    _jsonSchema.Maximum = validate as double?; // -> ValidateRange
                    break;
                case JsonValidate.MaxLength:
                    _jsonSchema.MaximumLength = validate as long?; // -> ValidateLength 
                    break;
                case JsonValidate.MinLength:
                    _jsonSchema.MinimumLength = validate as long?; // -> ValidateLength
                    break;
                case JsonValidate.Pattern:
                    _jsonSchema.Pattern = validate as string;      // -> ValidatePattern
                    break;
                case JsonValidate.MaxItems:
                    _jsonSchema.MaximumItems = validate as int?;   // -> ValidateCount
                    break;
                case JsonValidate.MinItems:
                    _jsonSchema.MinimumItems = validate as int?;   // -> ValidateCount
                    break;
                case JsonValidate.EnumSet:
                    JArray.FromObject(validate).ToList().ForEach(x => _jsonSchema.Enum.Add(x)); // -> ValidateSet
                    break;

            }

        }

        public void RemoveValidate(JsonValidate name)
        {
            Validates.Remove(name);

            switch (name)
            {
                case JsonValidate.Minimum:
                    _jsonSchema.Minimum = null;
                    break;
                case JsonValidate.Maximum:
                    _jsonSchema.Maximum = null;
                    break;
                case JsonValidate.MaxLength:
                    _jsonSchema.MaximumLength = null;
                    break;
                case JsonValidate.MinLength:
                    _jsonSchema.MinimumLength = null;
                    break;
                case JsonValidate.Pattern:
                    _jsonSchema.Pattern = null;
                    break;
                case JsonValidate.MaxItems:
                    _jsonSchema.MaximumItems = null;
                    break;
                case JsonValidate.MinItems:
                    _jsonSchema.MinimumItems = null;
                    break;
                case JsonValidate.EnumSet:
                    _jsonSchema.Enum.Clear();
                    break;

            }

        }

        /// <summary>
        /// Initialize PSParamter object
        /// </summary>
        /// <param name="name">Name of parameter</param>
        public PSParameter(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initialize PSParamter object
        /// </summary>
        /// <param name="name">Name of parameter</param>
        /// <param name="objectType">.Net type of paramter</param>
        public PSParameter(string name, Type objectType)
        {
            Name = name;
            Type = objectType;
        }

        /// <summary>
        /// Get parameter schema OpenApi schema in Json format
        /// </summary>
        /// <returns></returns>
        public JSchema GetJsonSchema()
        {
            _jsonSchema.Description = Description;
            _jsonSchema.Default = DefaultValue == null ? null : JToken.FromObject(DefaultValue);

            return _jsonSchema;
        }

        /// <summary>
        /// Get parameter schema OpenApi schema in Open API format
        /// </summary>
        /// <returns></returns>
        public OpenApiSchema GetSchemaOpenApiSchema()
        {
            var jsonSchema = GetJsonSchema();

            IOpenApiAny defaultValue = null;
            if (DefaultValue != null && ConvertObjectToOpenApi.ContainsKey(DefaultValue.GetType()))
                defaultValue = ConvertObjectToOpenApi[DefaultValue.GetType()](DefaultValue);

            return new OpenApiSchema
            {
                Description = jsonSchema.Description,
                Type = jsonSchema.Type.ToString().ToLower(),
                Items = jsonSchema.Items.Select(x => new OpenApiSchema { Type = x.Type.ToString().ToLower() }).FirstOrDefault(),
                Default = defaultValue,
                //MultipleOf = (decimal?)jsonSchema.MultipleOf; // -> N/A in PowerShell Attribute
                Minimum = (decimal?)jsonSchema.Minimum, // -> ValidateRange
                Maximum = (decimal?)jsonSchema.Maximum, // -> ValidateRange
                //ExclusiveMinimum = (bool)jsonSchema.ExclusiveMinimum; // -> N/A in PowerShell Attribute
                //ExclusiveMaximum = (bool)jsonSchema.ExclusiveMaximum; // -> N/A in PowerShell Attribute
                MaxLength = (int?)jsonSchema.MaximumLength, // -> ValidateLength
                MinLength = (int?)jsonSchema.MinimumLength, // -> ValidateLength
                Pattern = jsonSchema.Pattern,      // -> ValidatePattern
                MinItems = (int?)jsonSchema.MinimumItems,   // -> ValidateCount
                MaxItems = (int?)jsonSchema.MaximumItems,   // -> ValidateCount
                //UniqueItems = (bool)jsonSchema.UniqueItems; // -> N/A in PowerShell Attribute
                //MaxProperties = (int?)jsonSchema.MaximumProperties; // -> N/A in PowerShell Attribute
                //MinProperties = (int?)jsonSchema.MaximumProperties; // -> N/A in PowerShell Attribute
                Enum = _jsonSchema.Enum.Values<string>().ToList().Select(x => (IOpenApiAny)new OpenApiString(x)).ToList() // -> ValidateSet
            };
        }
    }
}
