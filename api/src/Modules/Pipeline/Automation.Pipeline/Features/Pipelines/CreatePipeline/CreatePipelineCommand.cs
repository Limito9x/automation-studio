namespace Automation.Pipeline.Features.Pipelines.CreatePipeline;

using Automation.Pipeline.Domain.Enums;

public record CreatePipelineCommand(
    Guid ProjectId,
    string Name,
    PipelineTriggerType TriggerType = PipelineTriggerType.Manual,
    Guid? TriggerWorkspaceId = null,
    System.Text.Json.JsonDocument? TriggerConfig = null
);
