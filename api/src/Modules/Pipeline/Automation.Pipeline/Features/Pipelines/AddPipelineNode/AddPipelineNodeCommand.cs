namespace Automation.Pipeline.Features.Pipelines.AddPipelineNode;

public record AddPipelineNodeCommand(
    Guid PipelineId,
    string RefId,
    string Kind,
    float PositionX,
    float PositionY,
    Dictionary<string, object?>? ConfigValues = null
);
