using System.Text.Json;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Domain.Entities;

public class PipelineNode : AuditableEntity
{
    public Guid PipelineId { get; set; }
    public Pipeline Pipeline { get; set; } = null!;

    // Trong thiết kế chỉ để refId
    // Vì node có thể là tool hoặc node def (user custom) -> generic
    public string RefId { get; set; } = string.Empty;
    public string Kind { get; set; } = PipelineNodeKind.Custom;
    public NodePosition Position { get; set; } = new(0, 0);
    public JsonDocument? Config { get; set; }

    public PipelineNode() { }

    public PipelineNode(
        Guid id,
        Guid pipelineId,
        string refId,
        string kind,
        float positionX,
        float positionY,
        JsonDocument? config = null
    )
    {
        Id = id != Guid.Empty ? id : IdGenerator.NewId();
        PipelineId = pipelineId;
        RefId = refId;
        Kind = kind;
        Position = new NodePosition(positionX, positionY);
        Config = config;
    }

    public void Update(float x, float y)
    {
        Position = new NodePosition(x, y);
    }

    public void UpdateConfig(JsonDocument? config)
    {
        Config = config;
    }
}

public record NodePosition(float X, float Y);

