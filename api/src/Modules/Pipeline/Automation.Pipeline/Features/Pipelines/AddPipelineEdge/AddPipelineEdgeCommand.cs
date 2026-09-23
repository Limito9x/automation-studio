namespace Automation.Pipeline.Features.Pipelines.AddPipelineEdge;

public record AddPipelineEdgeCommand(
    Guid PipelineId,
    Guid SourcePipelineNodeId,
    string SourcePin,
    Guid TargetPipelineNodeId,
    string TargetPin
);
