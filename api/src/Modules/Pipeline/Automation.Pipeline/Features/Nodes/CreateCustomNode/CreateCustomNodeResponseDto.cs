using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Features.Nodes.CreateCustomNode;

public record CreateCustomNodeResponseDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string Key,
    string Label,
    string Executor,
    IReadOnlyList<PinDefinition> Inputs,
    IReadOnlyList<PinDefinition> Outputs,
    DateTimeOffset CreatedAt
);
