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




        public string Name { get; private set; }

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
        public JSchemaType JsonType
        {
            get
            {
                return (JSchemaType)_jsonSchema.Type;
            }
        }


        public int Position { get; set; }


        // PowerShell Parameter attributes
        public string HelpMessage { get; set; } = "";

        public bool Required { get; set; } = false;

        public bool Hidden { get; set; } = false;

        public bool Switch { get; set; } = false;

        public bool AllowNull { get; set; } = true;

        public bool AllowEmpty { get; set; } = true;

        public string DefaultValue { get; set; } = "";

        public string Description { get; set; } = "";

        public RestLocation Location { get; set; } = Constants.DefaultParameterLocation;
        /* 

        * PowerShell Validation attributes
        ValidateCount
        ValidateLength
        ValidatePattern
        ValidateRange
        ValidateSet
        ValidateNotNullOrEmpty

        * JSON Schema definition
        * https://github.com/OAI/OpenAPI-Specification/blob/master/versions/3.0.0.md#schemaObject
        multipleOf -> N/A
        maximum -> ValidateRange
        exclusiveMaximum -> N/A
        minimum -> ValidateRange
        exclusiveMinimum -> N/A
        maxLength -> ValidateLength
        minLength -> ValidateLength
        pattern -> ValidatePattern
        maxItems -> ValidateCount
        minItems -> ValidateCount
        uniqueItems -> N/A
        maxProperties -> N/A
        minProperties -> N/A
        required -> Mandatory
        enum -> ValidateSet
        */

        public Dictionary<string, object> Validates = new Dictionary<string, object>();

        public void AddValidate(string name, object validate)
        {
            Validates.Add(name, validate);

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
            */

            /* ***** Not available with PowerShell Validate Attributes *****
            if (name.Equals("multipleOf",StringComparison.CurrentCultureIgnoreCase))
                _JsonSchema.MultipleOf = (double)validate;
            if (name.Equals("uniqueItems",StringComparison.CurrentCultureIgnoreCase))
                _JsonSchema.UniqueItems = (bool)validate; // -> N/A
            if (name.Equals("maxProperties",StringComparison.CurrentCultureIgnoreCase))
                _JsonSchema.MaxProperties = (int)validate; // -> N/A
            if (name.Equals("minProperties",StringComparison.CurrentCultureIgnoreCase))
                _JsonSchema.MinProperties = (int)validate; // -> N/A
                */

        }

        public void RemoveValidate(string name)
        {
            Validates.Remove(name);

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

        public PSParameter(string name)
        {
            Name = name;
        }

        public PSParameter(string name, Type objectType)
        {
            Name = name;
            Type = objectType;
        }

        public JSchema GetJsonSchema()
        {
            _jsonSchema.Description = Description;
            _jsonSchema.Default = DefaultValue;

            return _jsonSchema;
        }

        public OpenApiSchema GetSchemaOpenApiSchema()
        {

            return new OpenApiSchema
            {
                Description = _jsonSchema.Description,
                Type = _jsonSchema.Type.ToString().ToLower(),

                Default = new OpenApiString(DefaultValue),
                //schema.MultipleOf = (decimal?)_jsonSchema.MultipleOf; // -> N/A in PowerShell Attribute
                Minimum = (decimal?)_jsonSchema.Minimum, // -> ValidateRange
                Maximum = (decimal?)_jsonSchema.Maximum, // -> ValidateRange
                                                         //schema.ExclusiveMinimum = (bool)_jsonSchema.ExclusiveMinimum; // -> N/A in PowerShell Attribute
                                                         //schema.ExclusiveMaximum = (bool)_jsonSchema.ExclusiveMaximum; // -> N/A in PowerShell Attribute
                MaxLength = (int?)_jsonSchema.MaximumLength, // -> ValidateLength
                MinLength = (int?)_jsonSchema.MinimumLength, // -> ValidateLength
                Pattern = _jsonSchema.Pattern,      // -> ValidatePattern
                MinItems = (int?)_jsonSchema.MinimumItems,   // -> ValidateCount
                MaxItems = (int?)_jsonSchema.MaximumItems,   // -> ValidateCount
                                                             //schema.UniqueItems = (bool)_jsonSchema.UniqueItems; // -> N/A in PowerShell Attribute
                                                             //schema.MaxProperties = (int?)_jsonSchema.MaximumProperties; // -> N/A in PowerShell Attribute
                                                             //schema.MinProperties = (int?)_jsonSchema.MaximumProperties; // -> N/A in PowerShell Attribute
                Enum = _jsonSchema.Enum.Values<string>().ToList().Select(x => (IOpenApiAny)new OpenApiString(x)).ToList() // -> ValidateSet
            };
        }

    }
}
