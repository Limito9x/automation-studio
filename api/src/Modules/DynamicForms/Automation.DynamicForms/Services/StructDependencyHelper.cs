using System.Text.Json;

namespace Automation.DynamicForms.Services;

public static class StructDependencyHelper
{
    public static List<Guid> ExtractDirectStructIds(JsonDocument? fields)
    {
        if (fields == null) return [];

        var directIds = new HashSet<Guid>();
        ExtractStructIdsRecursive(fields.RootElement, directIds);
        return directIds.ToList();
    }

    private static void ExtractStructIdsRecursive(JsonElement element, HashSet<Guid> result)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ExtractStructIdsRecursive(item, result);
            }
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            // Check if this object represents a field definition
            if (element.TryGetProperty("properties", out var props) && props.ValueKind == JsonValueKind.Object)
            {
                if (props.TryGetProperty("structId", out var structIdProp))
                {
                    if (structIdProp.ValueKind == JsonValueKind.String &&
                        Guid.TryParse(structIdProp.GetString(), out var structId) &&
                        structId != Guid.Empty)
                    {
                        result.Add(structId);
                    }
                }
            }

            // Also search in any child properties or nested items arrays (e.g. nested field groups)
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind is JsonValueKind.Array or JsonValueKind.Object)
                {
                    // Avoid re-scanning properties object itself
                    if (property.NameEquals("properties")) continue;
                    ExtractStructIdsRecursive(property.Value, result);
                }
            }
        }
    }

    public static Result DetectCycle(
        Guid currentStructId,
        IEnumerable<Guid> directDependencies,
        IReadOnlyDictionary<Guid, IEnumerable<Guid>> graph)
    {
        var visited = new HashSet<Guid>();
        var recursionStack = new HashSet<Guid> { currentStructId };

        foreach (var childId in directDependencies)
        {
            if (childId == currentStructId)
            {
                return Result.Fail(new Error($"Circular dependency detected: Struct '{currentStructId}' cannot reference itself."));
            }

            var cycleResult = CheckCycleDfs(childId, currentStructId, graph, visited, recursionStack);
            if (cycleResult.IsFailed)
            {
                return cycleResult;
            }
        }

        return Result.Ok();
    }

    private static Result CheckCycleDfs(
        Guid node,
        Guid rootTarget,
        IReadOnlyDictionary<Guid, IEnumerable<Guid>> graph,
        HashSet<Guid> visited,
        HashSet<Guid> recursionStack)
    {
        if (node == rootTarget)
        {
            return Result.Fail(new Error($"Circular dependency detected: Reference path leads back to root Struct '{rootTarget}'."));
        }

        if (recursionStack.Contains(node))
        {
            return Result.Fail(new Error($"Circular dependency detected: Cycle involving Struct '{node}'."));
        }

        if (visited.Contains(node))
        {
            return Result.Ok();
        }

        visited.Add(node);
        recursionStack.Add(node);

        if (graph.TryGetValue(node, out var neighbors) && neighbors != null)
        {
            foreach (var neighbor in neighbors)
            {
                var result = CheckCycleDfs(neighbor, rootTarget, graph, visited, recursionStack);
                if (result.IsFailed) return result;
            }
        }

        recursionStack.Remove(node);
        return Result.Ok();
    }

    public static async Task<Result> DetectCycleAsync(
        Guid currentStructId,
        IEnumerable<Guid> directDependencies,
        Func<Guid, CancellationToken, Task<IEnumerable<Guid>?>> getChildDependenciesFunc,
        CancellationToken ct = default)
    {
        var visited = new HashSet<Guid>();
        var recursionStack = new HashSet<Guid> { currentStructId };

        foreach (var childId in directDependencies)
        {
            if (childId == currentStructId)
            {
                return Result.Fail(new Error($"Circular dependency detected: Struct '{currentStructId}' cannot reference itself."));
            }

            var cycleResult = await CheckCycleDfsAsync(childId, currentStructId, getChildDependenciesFunc, visited, recursionStack, ct);
            if (cycleResult.IsFailed)
            {
                return cycleResult;
            }
        }

        return Result.Ok();
    }

    private static async Task<Result> CheckCycleDfsAsync(
        Guid node,
        Guid rootTarget,
        Func<Guid, CancellationToken, Task<IEnumerable<Guid>?>> getChildDependenciesFunc,
        HashSet<Guid> visited,
        HashSet<Guid> recursionStack,
        CancellationToken ct)
    {
        if (node == rootTarget)
        {
            return Result.Fail(new Error($"Circular dependency detected: Reference path leads back to root Struct '{rootTarget}'."));
        }

        if (recursionStack.Contains(node))
        {
            return Result.Fail(new Error($"Circular dependency detected: Cycle involving Struct '{node}'."));
        }

        if (visited.Contains(node))
        {
            return Result.Ok();
        }

        visited.Add(node);
        recursionStack.Add(node);

        var neighbors = await getChildDependenciesFunc(node, ct);
        if (neighbors != null)
        {
            foreach (var neighbor in neighbors)
            {
                var result = await CheckCycleDfsAsync(neighbor, rootTarget, getChildDependenciesFunc, visited, recursionStack, ct);
                if (result.IsFailed) return result;
            }
        }

        recursionStack.Remove(node);
        return Result.Ok();
    }

    public static Result<List<Guid>> ComputeTransitiveDependencies(
        Guid currentStructId,
        IEnumerable<Guid> directDependencies,
        IReadOnlyDictionary<Guid, IEnumerable<Guid>> graph)
    {
        var cycleCheck = DetectCycle(currentStructId, directDependencies, graph);
        if (cycleCheck.IsFailed)
        {
            return Result.Fail<List<Guid>>(cycleCheck.Errors);
        }

        var closure = new HashSet<Guid>();
        var queue = new Queue<Guid>(directDependencies);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == currentStructId) continue;

            if (closure.Add(current))
            {
                if (graph.TryGetValue(current, out var children) && children != null)
                {
                    foreach (var child in children)
                    {
                        if (!closure.Contains(child))
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        return Result.Ok(closure.ToList());
    }

    public static async Task<Result<List<Guid>>> ComputeTransitiveDependenciesAsync(
        Guid currentStructId,
        IEnumerable<Guid> directDependencies,
        Func<Guid, CancellationToken, Task<IEnumerable<Guid>?>> getChildDependenciesFunc,
        CancellationToken ct = default)
    {
        var cycleCheck = await DetectCycleAsync(currentStructId, directDependencies, getChildDependenciesFunc, ct);
        if (cycleCheck.IsFailed)
        {
            return Result.Fail<List<Guid>>(cycleCheck.Errors);
        }

        var closure = new HashSet<Guid>();
        var queue = new Queue<Guid>(directDependencies);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == currentStructId) continue;

            if (closure.Add(current))
            {
                var children = await getChildDependenciesFunc(current, ct);
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        if (!closure.Contains(child))
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        return Result.Ok(closure.ToList());
    }
}
