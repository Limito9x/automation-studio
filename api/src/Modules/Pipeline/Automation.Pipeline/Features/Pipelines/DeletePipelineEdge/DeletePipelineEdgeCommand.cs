namespace Automation.Pipeline.Features.Pipelines.DeletePipelineEdge;

public record DeletePipelineEdgeCommand(
    Guid PipelineId,
    Guid EdgeId
);
