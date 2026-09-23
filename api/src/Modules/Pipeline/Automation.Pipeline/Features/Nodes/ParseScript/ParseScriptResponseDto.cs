using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Features.Nodes.ParseScript;

public record ParseScriptResponseDto(
    string SuggestedName,
    string SuggestedLabel,
    string Executor,
    string? Description,
    IReadOnlyList<PinDefinition> Inputs,
    IReadOnlyList<PinDefinition> Outputs
);
