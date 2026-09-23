using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Features.Nodes.CreateCustomNode;

public record CreateCustomNodeCommand(
    Guid ProjectId,
    string Name,
    string? Label,
    string? Executor,
    Guid? AssetId,
    string? OriginalFileName,
    List<PinDefinition>? Inputs,
    List<PinDefinition>? Outputs
);
