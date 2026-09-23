using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Features.Pipelines.GetStructCatalogue;

public record StructDefinitionDto(
    string StructType,
    string Label,
    IReadOnlyList<PinDefinition> OutputPins
);
