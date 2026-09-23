using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Features.Nodes.GetNodePalette;

public record NodePaletteItemDto(
    string Key,
    string Label,
    string Category,
    string Source, // "BuiltIn" | "Custom"
    string Executor,
    IReadOnlyList<PinDefinition> Inputs,
    IReadOnlyList<PinDefinition> Outputs,
    Guid? Id = null
);
