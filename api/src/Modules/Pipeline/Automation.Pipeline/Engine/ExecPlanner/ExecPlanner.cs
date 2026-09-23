using System.Text.Json;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Engine.Models;
using Automation.Pipeline.Tools;

namespace Automation.Pipeline.Engine.ExecPlanner;

public class ExecPlanner : IExecPlanner
{
    public ExecPlan BuildExecPlan(
        Domain.Entities.Pipeline pipeline,
        IReadOnlyList<NodeDefinition> customDefinitions,
        IToolRegistry toolRegistry,
        Dictionary<string, object?>? runtimeInputs = null
    )
    {
        var customDefsLookup = customDefinitions
            .GroupBy(x => x.Id.ToString())
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var customDefsKeyLookup = customDefinitions
            .Where(x => !string.IsNullOrEmpty(x.Key))
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var execEdges = pipeline.Edges
            .Where(e => e.Kind == EdgeKind.Exec ||
                        IsLoopBodyPin(e.SourcePin) ||
                        IsCompletedPin(e.SourcePin) ||
                        (!string.IsNullOrWhiteSpace(e.SourcePin) && IsExecOutPin(e.SourcePin)) ||
                        (!string.IsNullOrWhiteSpace(e.TargetPin) && IsExecInPin(e.TargetPin)))
            .ToList();

        var cycleNodeIds = new List<string>();
        var segments = new List<ExecSegment>();

        // 1. Build Exec Steps Lookup (Action & FlowControl nodes only)
        var stepsLookup = new Dictionary<Guid, ExecStep>();
        foreach (var node in pipeline.Nodes)
        {
            var isStart = string.Equals(node.Kind, PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(node.RefId, "Start", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(node.RefId, "BeginExecute", StringComparison.OrdinalIgnoreCase);

            var isFlowControl = string.Equals(node.Kind, PipelineNodeKind.FlowControl, StringComparison.OrdinalIgnoreCase) ||
                                (toolRegistry.Get(node.RefId) is { } t && string.Equals(t.Category, "Flow Control", StringComparison.OrdinalIgnoreCase));

            var tool = toolRegistry.Get(node.RefId);
            var isPure = tool is { IsPure: true };

            // Pure nodes are excluded from ExecPlan (resolved on-demand via pull)
            if (isPure && !isStart && !isFlowControl)
            {
                continue;
            }

            IReadOnlyList<PinDefinition> inputs = [];
            IReadOnlyList<PinDefinition> outputs = [];
            var label = node.RefId;
            var executor = "dotNet";

            if (isStart)
            {
                label = "Start";
                executor = "dotNet";
                outputs = pipeline.Inputs.OrderBy(i => i.Order).Select(i => new PinDefinition
                {
                    Id = i.Key,
                    Label = i.Label,
                    Kind = PinKind.Data,
                    PrimitiveType = i.Type,
                    Cardinality = i.Cardinality,
                    IsRequired = i.IsRequired,
                    DefaultValue = i.DefaultValue
                }).ToList();
            }
            else if (tool != null)
            {
                inputs = tool.Inputs;
                outputs = tool.Outputs;
                label = !string.IsNullOrWhiteSpace(tool.Label) ? tool.Label : tool.Key;
                executor = "dotNet";
            }
            else
            {
                NodeDefinition? def = null;
                if (customDefsLookup.TryGetValue(node.RefId, out var foundDef) ||
                    customDefsKeyLookup.TryGetValue(node.RefId, out foundDef))
                {
                    def = foundDef;
                }

                if (def != null)
                {
                    inputs = def.Inputs;
                    outputs = def.Outputs;
                    label = !string.IsNullOrEmpty(def.Label) ? def.Label : def.Name;
                    executor = !string.IsNullOrEmpty(def.Executor) ? def.Executor : "blender";
                }
            }

            var incoming = pipeline.Edges
                .Where(e => e.TargetPipelineNodeId == node.Id)
                .Select(e => new IncomingPinConnection(e.TargetPin, e.SourcePipelineNodeId, e.SourcePin))
                .ToList();

            var nodeKind = isStart ? PipelineNodeKind.Start :
                           isFlowControl ? PipelineNodeKind.FlowControl :
                           tool != null ? PipelineNodeKind.Tool :
                           node.Kind;

            stepsLookup[node.Id] = new ExecStep
            {
                NodeId = node.Id,
                RefId = node.RefId,
                Kind = nodeKind,
                Label = label,
                Executor = executor,
                InputPins = inputs,
                OutputPins = outputs,
                IncomingConnections = incoming,
                Config = node.Config
            };
        }

        // 2. Discover Entry Point and Trace Exec Chain
        var execTargets = execEdges.Select(e => e.TargetPipelineNodeId).ToHashSet();
        var entryNode = pipeline.Nodes.FirstOrDefault(n => string.Equals(n.Kind, PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase))
                        ?? pipeline.Nodes.FirstOrDefault(n => string.Equals(n.RefId, "BeginExecute", StringComparison.OrdinalIgnoreCase))
                        ?? pipeline.Nodes.FirstOrDefault(n => stepsLookup.ContainsKey(n.Id) && !execTargets.Contains(n.Id))
                        ?? pipeline.Nodes.FirstOrDefault(n => stepsLookup.ContainsKey(n.Id));

        if (entryNode != null && stepsLookup.TryGetValue(entryNode.Id, out var startStep))
        {
            var visited = new HashSet<Guid>();
            var recursionStack = new HashSet<Guid>();

            segments = TraceExecChain(startStep.NodeId, stepsLookup, execEdges, visited, recursionStack, cycleNodeIds);
        }

        // 3. Pre-flight Pin Validation
        var unresolvedPins = ValidateRequiredPins(segments, runtimeInputs);

        return new ExecPlan
        {
            Segments = segments,
            CycleNodeIds = cycleNodeIds,
            UnresolvedPins = unresolvedPins
        };
    }

    private List<ExecSegment> TraceExecChain(
        Guid? startNodeId,
        Dictionary<Guid, ExecStep> stepsLookup,
        List<PipelineEdge> execEdges,
        HashSet<Guid> visited,
        HashSet<Guid> recursionStack,
        List<string> cycleNodeIds
    )
    {
        var segments = new List<ExecSegment>();
        ExecSegment? currentSegment = null;
        var currentNodeId = startNodeId;

        while (currentNodeId.HasValue)
        {
            var cId = currentNodeId.Value;

            if (recursionStack.Contains(cId))
            {
                cycleNodeIds.Add(cId.ToString());
                break;
            }

            if (!stepsLookup.TryGetValue(cId, out var step) || !visited.Add(cId))
            {
                break;
            }

            recursionStack.Add(cId);

            var isFlowControl = string.Equals(step.Kind, PipelineNodeKind.FlowControl, StringComparison.OrdinalIgnoreCase);

            if (isFlowControl)
            {
                // Finalize current open segment
                if (currentSegment != null && currentSegment.Steps.Count > 0)
                {
                    segments.Add(currentSegment);
                    currentSegment = null;
                }

                // Create dedicated segment for FlowControl
                var fcSegment = new ExecSegment(step.Executor, isFlowControl: true)
                {
                    Steps = [step]
                };

                // Trace loop_body sub-plan recursively
                var loopBodyEdge = execEdges.FirstOrDefault(e =>
                    e.SourcePipelineNodeId == cId &&
                    IsLoopBodyPin(e.SourcePin));

                if (loopBodyEdge != null)
                {
                    var bodyVisited = new HashSet<Guid>(visited);
                    var bodyStack = new HashSet<Guid>(recursionStack);
                    var bodySegments = TraceExecChain(loopBodyEdge.TargetPipelineNodeId, stepsLookup, execEdges, bodyVisited, bodyStack, cycleNodeIds);
                    fcSegment.BodyPlan = new ExecPlan { Segments = bodySegments };
                }

                segments.Add(fcSegment);

                // Continue along completed pin
                var completedEdge = execEdges.FirstOrDefault(e =>
                    e.SourcePipelineNodeId == cId &&
                    IsCompletedPin(e.SourcePin));

                recursionStack.Remove(cId);
                currentNodeId = completedEdge?.TargetPipelineNodeId;
                continue;
            }

            // Normal Action step: Group by executor
            if (currentSegment == null || !string.Equals(currentSegment.Executor, step.Executor, StringComparison.OrdinalIgnoreCase))
            {
                if (currentSegment != null && currentSegment.Steps.Count > 0)
                {
                    segments.Add(currentSegment);
                }
                currentSegment = new ExecSegment(step.Executor) { Steps = [step] };
            }
            else
            {
                currentSegment.Steps.Add(step);
            }

            // Follow exec_out to next action node
            var nextEdge = execEdges.FirstOrDefault(e =>
                e.SourcePipelineNodeId == cId &&
                IsExecOutPin(e.SourcePin));

            currentNodeId = nextEdge?.TargetPipelineNodeId;
        }

        if (currentSegment != null && currentSegment.Steps.Count > 0)
        {
            segments.Add(currentSegment);
        }

        return segments;
    }

    private static bool IsLoopBodyPin(string? pin)
    {
        if (string.IsNullOrWhiteSpace(pin)) return false;
        var norm = pin.Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        return norm is "loopbody" or "body" or "loop";
    }

    private static bool IsCompletedPin(string? pin)
    {
        if (string.IsNullOrWhiteSpace(pin)) return false;
        var norm = pin.Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        return norm is "completed" or "done" or "complete";
    }

    /// <summary>
    /// For use in TraceExecChain (following next step): empty pin = also exec edge
    /// </summary>
    private static bool IsExecOutPin(string? pin)
    {
        if (string.IsNullOrWhiteSpace(pin)) return true;
        var norm = pin.Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        return norm is "execout" or "exec";
    }

    /// <summary>
    /// For use in filtering execEdges: non-empty pin must explicitly be an exec-in pin
    /// </summary>
    private static bool IsExecInPin(string? pin)
    {
        if (string.IsNullOrWhiteSpace(pin)) return false;
        var norm = pin.Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        return norm is "execin" or "exec";
    }

    private static List<UnresolvedPin> ValidateRequiredPins(
        List<ExecSegment> segments,
        Dictionary<string, object?>? runtimeInputs
    )
    {
        var unresolvedPins = new List<UnresolvedPin>();

        IEnumerable<ExecStep> Flatten(IEnumerable<ExecSegment> segs)
        {
            foreach (var seg in segs)
            {
                foreach (var step in seg.Steps) yield return step;
                if (seg.BodyPlan != null)
                {
                    foreach (var s in Flatten(seg.BodyPlan.Segments)) yield return s;
                }
            }
        }

        foreach (var step in Flatten(segments))
        {
            foreach (var pin in step.InputPins)
            {
                if (pin.Kind == PinKind.Exec || string.Equals(pin.Id, "exec_in", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var isConnected = step.IncomingConnections.Any(c =>
                    string.Equals(c.TargetPinKey, pin.Id, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.TargetPinKey, pin.Label, StringComparison.OrdinalIgnoreCase));

                var hasInlineValue = HasConfigValue(step.Config, pin.Id) || HasConfigValue(step.Config, pin.Label);
                var hasDefaultValue = pin.DefaultValue != null;
                var hasRuntimeInput = runtimeInputs != null &&
                    (runtimeInputs.ContainsKey(pin.Id) ||
                     runtimeInputs.ContainsKey(pin.Label) ||
                     runtimeInputs.ContainsKey($"{step.NodeId}:{pin.Id}"));

                var isScopeImplicit = string.Equals(pin.Id, "Item", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(pin.Id, "Index", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(pin.Id, "Key", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(pin.Id, "Value", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(pin.Id, "YieldValue", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(pin.Id, "YieldKey", StringComparison.OrdinalIgnoreCase);

                if (!isConnected && !hasInlineValue && !hasDefaultValue && !hasRuntimeInput && !isScopeImplicit && pin.IsRequired)
                {
                    unresolvedPins.Add(new UnresolvedPin(
                        step.NodeId,
                        step.Label,
                        pin.Id,
                        pin.Label,
                        pin.PrimitiveType,
                        "Required input pin is not connected and has no inline or default value."
                    ));
                }
            }
        }

        return unresolvedPins;
    }

    private static bool HasConfigValue(JsonDocument? config, string key)
    {
        if (config == null || string.IsNullOrWhiteSpace(key)) return false;

        if (config.RootElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in config.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    return prop.Value.ValueKind switch
                    {
                        JsonValueKind.Null or JsonValueKind.Undefined => false,
                        JsonValueKind.String => !string.IsNullOrEmpty(prop.Value.GetString()),
                        JsonValueKind.Array => prop.Value.GetArrayLength() > 0,
                        _ => true
                    };
                }
            }
        }

        return false;
    }
}
