using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Tools;

namespace Automation.Pipeline.Engine.StructRegistry;

public interface IEntityStructDefinition
{
    string StructType { get; }
    string Label { get; }
    IReadOnlyList<PinDefinition> OutputPins { get; }

    Task<Dictionary<string, object>> ResolveAsync(
        object targetInput,
        ToolExecutionContext context
    );
}
