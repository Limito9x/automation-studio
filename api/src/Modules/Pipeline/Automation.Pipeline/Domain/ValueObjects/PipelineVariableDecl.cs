using Automation.Pipeline.Domain.Enums;

namespace Automation.Pipeline.Domain.ValueObjects;

public record PipelineVariableDecl
{
    public string Name { get; init; } = string.Empty;
    public PinPrimitiveType Type { get; init; } = PinPrimitiveType.String;
    public PinCardinality Cardinality { get; init; } = PinCardinality.Single;
    public string? Description { get; init; }
    public string? StructType { get; init; }
}
