namespace Automation.Pipeline.Features.Pipelines.UpdatePipelineInput;

public record UpdatePipelineInputCommand(
    Guid PipelineId,
    Guid InputId,
    string? Key = null,
    string? Label = null,
    string? Type = null,
    string? Cardinality = null,
    bool? IsRequired = null,
    string? DefaultValue = null,
    int? Order = null
);
