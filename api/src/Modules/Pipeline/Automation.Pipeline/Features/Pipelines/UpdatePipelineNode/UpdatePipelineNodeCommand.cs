namespace Automation.Pipeline.Features.Pipelines.UpdatePipelineNode;

public record UpdatePipelineNodeCommand(
    Guid PipelineId,
    Guid NodeId,
    float? PositionX = null,
    float? PositionY = null,
    Dictionary<string, object?>? ConfigValues = null
);
