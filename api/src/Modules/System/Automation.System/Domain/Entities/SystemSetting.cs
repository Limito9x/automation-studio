using Automation.SharedKernel.Domain.Entities;

namespace Automation.SystemModule.Domain.Entities;

public class SystemSetting : AuditableEntity
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string ValueType { get; set; } = default!;
    public string? Description { get; set; }
    
    public SystemSetting() { }
    
    public SystemSetting(string key, string value, string valueType, string? description = null)
    {
        Key = key;
        Value = value;
        ValueType = valueType;
        Description = description;
    }
    
    public void UpdateValue(string value)
    {
        Value = value;
    }
}



