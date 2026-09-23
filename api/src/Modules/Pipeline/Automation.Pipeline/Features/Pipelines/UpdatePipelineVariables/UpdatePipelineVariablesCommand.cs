using Automation.Pipeline.Features.Pipelines.Dtos;

namespace Automation.Pipeline.Features.Pipelines.UpdatePipelineVariables;

public record UpdatePipelineVariablesCommand(
    Guid PipelineId,
    List<PipelineVariableDto> Variables
);
