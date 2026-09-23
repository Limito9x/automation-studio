using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Features.Nodes.UpdateCustomNode;

public record UpdateCustomNodeCommand(
    Guid Id,
    string Name,
    string? Label,
    string? Executor,
    Guid? AssetId,
    string? OriginalFileName,
    List<PinDefinition>? Inputs,
    List<PinDefinition>? Outputs
);

public record UpdateCustomNodeRequest(
    string Name,
    string? Label,
    string? Executor,
    Guid? AssetId,
    string? OriginalFileName,
    List<PinDefinition>? Inputs,
    List<PinDefinition>? Outputs
);
