using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.Entities;

public class PipelineOutput : AuditableEntity
{
    public Guid PipelineId { get; set; }
    public Pipeline Pipeline { get; set; } = null!;
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public PinPrimitiveType Type { get; set; }
    public PinCardinality Cardinality { get; set; } = PinCardinality.Single;
    public int Order { get; set; }

    public PipelineOutput() { }

    public PipelineOutput(
        Guid pipelineId,
        string key,
        string label,
        PinPrimitiveType type,
        PinCardinality cardinality = PinCardinality.Single,
        int order = 0
    )
    {
        PipelineId = pipelineId;
        Key = key;
        Label = label;
        Type = type;
        Cardinality = cardinality;
        Order = order;
    }

    public void Update(
        string key,
        string label,
        PinPrimitiveType type,
        PinCardinality cardinality,
        int order
    )
    {
        Key = key;
        Label = label;
        Type = type;
        Cardinality = cardinality;
        Order = order;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
