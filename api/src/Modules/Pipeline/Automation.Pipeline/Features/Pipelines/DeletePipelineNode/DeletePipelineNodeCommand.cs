namespace Automation.Pipeline.Features.Pipelines.DeletePipelineNode;

public record DeletePipelineNodeCommand(
    Guid PipelineId,
    Guid NodeId
);
