namespace DynamicPowerShellApi.Configuration
{
    class PSValidateType
    {
        public const string multipleOf = "multipleOf"; // -> N/A
        public const string ValidateRange_Min = "minimum"; // -> ValidateRange
        public const string ValidateRange_Max = "maximum"; // -> ValidateRange
        //public const string exclusiveMinimum, // -> N/A
        //public const string exclusiveMaximum, // -> N/A
        public const string ValidateLength_Max = "maxLength"; // -> ValidateLength
        public const string ValidateLength_Min = "minLength"; // -> ValidateLength
        public const string ValidatePattern = "pattern";      // -> ValidatePattern
        public const string ValidateCount_Max = "maxItems";   // -> ValidateCount
        public const string ValidateCount_Min = "minItems";   // -> ValidateCount
        //public const string uniqueItems, // -> N/A
        //public const string maxProperties, // -> N/A
        //public const string minProperties, // -> N/A
        public const string Mandatory = "required"; // -> Mandatory
        public const string ValidateSet = "enum"; // -> ValidateSet
    }
}
