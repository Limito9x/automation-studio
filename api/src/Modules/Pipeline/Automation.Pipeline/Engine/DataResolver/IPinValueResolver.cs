using Automation.Pipeline.Engine.Models;

namespace Automation.Pipeline.Engine.DataResolver;

public interface IPinValueResolver
{
    Task<object?> ResolvePinAsync(
        Guid executionId,
        Guid nodeId,
        string pinKey,
        ScopeContext? scope = null,
        CancellationToken ct = default
    );

    Task<Dictionary<string, object?>> ResolveAllPinsAsync(
        Guid executionId,
        Guid nodeId,
        IEnumerable<string>? requestedPinKeys = null,
        ScopeContext? scope = null,
        CancellationToken ct = default
    );
}
