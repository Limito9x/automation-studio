using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Domain.Entities;

public class NodeDefinition : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<PinDefinition> Inputs { get; set; } = new();
    public List<PinDefinition> Outputs { get; set; } = new();
    public string Executor { get; set; } = string.Empty;

    public NodeDefinition() { }

    public NodeDefinition(
        Guid projectId,
        string name,
        string key,
        string label,
        string executor,
        List<PinDefinition> inputs,
        List<PinDefinition> outputs
    )
    {
        ProjectId = projectId;
        Name = name;
        Key = key;
        Label = label;
        Executor = executor;
        Inputs = inputs;
        Outputs = outputs;
    }

    public void Update(
        string name,
        string label,
        string executor,
        List<PinDefinition> inputs,
        List<PinDefinition> outputs
    )
    {
        Name = name;
        Label = label;
        Executor = executor;
        Inputs = inputs;
        Outputs = outputs;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Restore()
    {
        DeletedAt = null;
        DeletedBy = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
