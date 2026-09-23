namespace Automation.Pipeline.Features.Pipelines.DeletePipelineInput;

public record DeletePipelineInputCommand(
    Guid PipelineId,
    Guid InputId
);
