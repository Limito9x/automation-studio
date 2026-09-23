namespace Automation.SystemAbstractions;

public class RegisteredSystemSetting
{
    public string Key { get; }
    public string DefaultValue { get; }
    public string ValueType { get; }
    public string? Description { get; }

    public RegisteredSystemSetting(string key, string defaultValue, string valueType, string? description)
    {
        Key = key;
        DefaultValue = defaultValue;
        ValueType = valueType;
        Description = description;
    }
}



