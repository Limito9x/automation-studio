using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class PipelineInput : AuditableEntity
{
    public Guid PipelineId { get; set; }
    public Pipeline Pipeline { get; set; } = null!;
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public PinPrimitiveType Type { get; set; }
    public PinCardinality Cardinality { get; set; } = PinCardinality.Single;
    public bool IsRequired { get; set; } = true;
    public string? DefaultValue { get; set; }
    public int Order { get; set; }

    public PipelineInput() { }

    public PipelineInput(
        Guid pipelineId,
        string key,
        string label,
        PinPrimitiveType type,
        PinCardinality cardinality = PinCardinality.Single,
        bool isRequired = true,
        string? defaultValue = null,
        int order = 0
    )
    {
        PipelineId = pipelineId;
        Key = key;
        Label = label;
        Type = type;
        Cardinality = cardinality;
        IsRequired = isRequired;
        DefaultValue = defaultValue;
        Order = order;
    }

    public void Update(
        string key,
        string label,
        PinPrimitiveType type,
        PinCardinality cardinality,
        bool isRequired,
        string? defaultValue,
        int order
    )
    {
        Key = key;
        Label = label;
        Type = type;
        Cardinality = cardinality;
        IsRequired = isRequired;
        DefaultValue = defaultValue;
        Order = order;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
