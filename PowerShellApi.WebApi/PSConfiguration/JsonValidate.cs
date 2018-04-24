namespace PowerShellRestApi.PSConfiguration
{
    public enum JsonValidate
    {
        //* JSON Schema definition
        //* https://github.com/OAI/OpenAPI-Specification/blob/master/versions/3.0.0.md#schemaObject

        Minimum, //-> ValidateRange
        Maximum, //-> ValidateRange

        // ***** In PowerShell is always false *****
        ExclusiveMinimum, //-> N/A
        ExclusiveMaximum, //-> N/A

        MinLength, //-> ValidateLength
        MaxLength, //-> ValidateLength
        Pattern, //-> ValidatePattern
        MinItems, //-> ValidateCount
        MaxItems, //-> ValidateCount
        Required, //-> Mandatory
        EnumSet, //-> ValidateSet

        // ***** Not available with PowerShell Validate Attributes *****
        MultipleOf, //-> N/A
        UniqueItems, //-> N/A
        MinProperties, //-> N/A
        MaxProperties, //-> N/A
    }

}
