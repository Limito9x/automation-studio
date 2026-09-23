namespace Automation.Pipeline.Features.Pipelines.AddPipelineInput;

public record AddPipelineInputCommand(
    Guid PipelineId,
    string Key,
    string Label,
    string Type,
    string Cardinality = "Single",
    bool IsRequired = true,
    string? DefaultValue = null,
    int Order = 0
);
