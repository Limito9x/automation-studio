using System.Text.Json;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Engine.DataResolver;
using Automation.Pipeline.Engine.Models;
using Automation.Pipeline.Hubs;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Pipeline.Tools;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Engine.Orchestrator.Dispatchers;

public class DotNetSegmentDispatcher(
    PipelineDbContext db,
    IToolRegistry toolRegistry,
    IPinValueResolver pinResolver,
    IExecutionMemoryStore memoryStore,
    ILogger<DotNetSegmentDispatcher> logger,
    IHubContext<PipelineExecutionHub>? hubContext = null
)
{
    public async Task<Result> DispatchAsync(
        PipelineExecution execution,
        ExecSegment segment,
        ScopeContext? scope = null,
        IPipelineOrchestrator? orchestrator = null,
        CancellationToken ct = default
    )
    {
        foreach (var step in segment.Steps)
        {
            var isStart = string.Equals(step.Kind, PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(step.RefId, "Start", StringComparison.OrdinalIgnoreCase);

            if (isStart)
            {
                await RecordNodeSuccessAsync(execution.Id, execution.PipelineId, step.NodeId, new Dictionary<string, object>(), ct);
                continue;
            }

            var isReturn = string.Equals(step.Kind, PipelineNodeKind.Return, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(step.RefId, "Return", StringComparison.OrdinalIgnoreCase);

            if (isReturn)
            {
                var returnResolvedInputs = await pinResolver.ResolveAllPinsAsync(
                    execution.Id,
                    step.NodeId,
                    scope: scope,
                    ct: ct
                );

                var returnOutputs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var (k, v) in returnResolvedInputs)
                {
                    if (v != null) returnOutputs[k] = v;
                }

                var returnOutputsDict = returnOutputs.ToDictionary(k => k.Key, v => (object?)v.Value);
                await memoryStore.SetNodeAllOutputsAsync(execution.Id, step.NodeId, returnOutputsDict, scope, ct);
                await RecordNodeSuccessAsync(execution.Id, execution.PipelineId, step.NodeId, returnOutputs, ct);
                continue;
            }

            var isSubPipeline = string.Equals(step.Kind, PipelineNodeKind.SubPipeline, StringComparison.OrdinalIgnoreCase);

            await RecordNodeRunningAsync(execution.Id, execution.PipelineId, step.NodeId, ct);

            if (isSubPipeline)
            {
                var subResolvedInputs = await pinResolver.ResolveAllPinsAsync(
                    execution.Id,
                    step.NodeId,
                    scope: scope,
                    ct: ct
                );

                Guid? targetPipelineId = null;
                if (Guid.TryParse(step.RefId, out var parsedRefId))
                {
                    targetPipelineId = parsedRefId;
                }
                else if (step.Config != null)
                {
                    try
                    {
                        if (step.Config.RootElement.TryGetProperty("pipelineId", out var pProp) && pProp.TryGetGuid(out var gid))
                        {
                            targetPipelineId = gid;
                        }
                    }
                    catch { }
                }

                if (!targetPipelineId.HasValue || targetPipelineId.Value == Guid.Empty)
                {
                    var err = $"Sub-Pipeline target not configured for step '{step.Label}'.";
                    logger.LogError(err);
                    await RecordNodeFailureAsync(execution.Id, execution.PipelineId, step.NodeId, err, ct);
                    return Result.Fail(err);
                }

                var childPipeline = await db.Pipelines
                    .AsNoTracking()
                    .Include(p => p.Outputs)
                    .FirstOrDefaultAsync(p => p.Id == targetPipelineId.Value, ct);

                if (childPipeline == null)
                {
                    var err = $"Target Sub-Pipeline '{targetPipelineId.Value}' not found.";
                    logger.LogError(err);
                    await RecordNodeFailureAsync(execution.Id, execution.PipelineId, step.NodeId, err, ct);
                    return Result.Fail(err);
                }

                var childExecution = new PipelineExecution(childPipeline.Id, execution.AgentId);
                db.PipelineExecutions.Add(childExecution);
                await db.SaveChangesAsync(ct);

                var childInputs = new Dictionary<string, object?>(subResolvedInputs, StringComparer.OrdinalIgnoreCase);

                logger.LogInformation("Executing Sub-Pipeline [{ChildName}] ({ChildPipelineId}) under Execution {ExecutionId}",
                    childPipeline.Name, childPipeline.Id, execution.Id);

                if (orchestrator == null)
                {
                    var err = "Orchestrator instance required for Sub-Pipeline execution.";
                    logger.LogError(err);
                    await RecordNodeFailureAsync(execution.Id, execution.PipelineId, step.NodeId, err, ct);
                    return Result.Fail(err);
                }

                var childResult = await orchestrator.ExecuteOrResumeAsync(childExecution.Id, childInputs, ct);

                if (childResult.IsFailed || childResult.Value.Status == ExecutionStatus.Failed)
                {
                    var err = childResult.Errors.FirstOrDefault()?.Message ?? childResult.Value.ErrorMessage ?? "Sub-Pipeline execution failed.";
                    logger.LogError(err);
                    await RecordNodeFailureAsync(execution.Id, execution.PipelineId, step.NodeId, err, ct);
                    return Result.Fail(err);
                }

                // Collect outputs from child Return node (if any) or memory store
                var subOutputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                var childReturnNode = await db.PipelineNodes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(n => n.PipelineId == childPipeline.Id && n.Kind == PipelineNodeKind.Return, ct);

                if (childReturnNode != null)
                {
                    var childReturnNodeExec = await db.NodeExecutions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ne => ne.PipelineExecutionId == childExecution.Id && ne.PipelineNodeId == childReturnNode.Id, ct);

                    if (childReturnNodeExec?.Output != null)
                    {
                        try
                        {
                            var parsedOuts = JsonSerializer.Deserialize<Dictionary<string, object?>>(childReturnNodeExec.Output.RootElement.GetRawText());
                            if (parsedOuts != null)
                            {
                                foreach (var (k, v) in parsedOuts)
                                {
                                    subOutputs[k] = v;
                                }
                            }
                        }
                        catch { }
                    }
                }

                await memoryStore.SetNodeAllOutputsAsync(execution.Id, step.NodeId, subOutputs, scope, ct);
                var successOutputs = new Dictionary<string, object>();
                foreach (var (k, v) in subOutputs)
                {
                    successOutputs[k] = v ?? string.Empty;
                }
                await RecordNodeSuccessAsync(execution.Id, execution.PipelineId, step.NodeId, successOutputs, ct);
                continue;
            }

            var tool = toolRegistry.Get(step.RefId);
            if (tool == null)
            {
                var err = $"DotNet Tool '{step.RefId}' not found for step '{step.Label}'.";
                logger.LogError(err);
                return Result.Fail(err);
            }

            // 1. Pull all inputs on-demand
            var resolvedInputs = await pinResolver.ResolveAllPinsAsync(
                execution.Id,
                step.NodeId,
                scope: scope,
                ct: ct
            );

            // Cast to Dictionary<string, object>
            var toolInputs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in resolvedInputs)
            {
                if (v != null) toolInputs[k] = v;
            }

            // 2. Execute Tool
            var projectId = execution.Pipeline?.ProjectId ?? Guid.Empty;
            var toolContext = new ToolExecutionContext(execution.Id, execution.PipelineId, execution.AgentId, ct, step.NodeId, projectId);
            Dictionary<string, object> outputs;
            try
            {
                logger.LogInformation("Executing DotNet Tool [{ToolLabel}] ({NodeId})", step.Label, step.NodeId);
                outputs = await tool.ExecuteAsync(toolInputs, toolContext);
            }
            catch (Exception ex)
            {
                var err = $"Execution of tool '{step.Label}' failed: {ex.Message}";
                logger.LogError(ex, err);
                await RecordNodeFailureAsync(execution.Id, execution.PipelineId, step.NodeId, err, ct);
                return Result.Fail(err);
            }

            // 3. Save outputs to memory store for downstream pull
            var outputsDict = outputs.ToDictionary(k => k.Key, v => (object?)v.Value);
            await memoryStore.SetNodeAllOutputsAsync(execution.Id, step.NodeId, outputsDict, scope, ct);

            // 4. Record success in DB
            await RecordNodeSuccessAsync(execution.Id, execution.PipelineId, step.NodeId, outputs, ct);
        }

        return Result.Ok();
    }

    private async Task RecordNodeSuccessAsync(Guid executionId, Guid pipelineId, Guid nodeId, Dictionary<string, object> outputs, CancellationToken ct)
    {
        var nodeExec = await db.NodeExecutions
            .FirstOrDefaultAsync(x => x.PipelineExecutionId == executionId && x.PipelineNodeId == nodeId, ct);

        var outputJson = JsonSerializer.Serialize(outputs);
        var outputDoc = JsonDocument.Parse(outputJson);

        if (nodeExec == null)
        {
            nodeExec = new NodeExecution(executionId, nodeId, status: ExecutionStatus.Running);
            nodeExec.MarkSucceeded(outputDoc);
            db.NodeExecutions.Add(nodeExec);
        }
        else
        {
            nodeExec.MarkSucceeded(outputDoc);
        }

        await db.SaveChangesAsync(ct);

        if (hubContext != null)
        {
            try
            {
                await hubContext.Clients.Group($"pipeline_{pipelineId}").SendAsync(
                    "PipelineNodeExecutionUpdated",
                    new { executionId, pipelineId, nodeId, status = "succeeded" },
                    ct
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to broadcast PipelineNodeExecutionUpdated via SignalR for {NodeId}", nodeId);
            }
        }
    }

    private async Task RecordNodeRunningAsync(Guid executionId, Guid pipelineId, Guid nodeId, CancellationToken ct)
    {
        var nodeExec = await db.NodeExecutions
            .FirstOrDefaultAsync(x => x.PipelineExecutionId == executionId && x.PipelineNodeId == nodeId, ct);

        if (nodeExec == null)
        {
            nodeExec = new NodeExecution(executionId, nodeId, status: ExecutionStatus.Running);
            nodeExec.MarkRunning();
            db.NodeExecutions.Add(nodeExec);
            await db.SaveChangesAsync(ct);
        }
        else if (nodeExec.Status != ExecutionStatus.Running)
        {
            nodeExec.MarkRunning();
            await db.SaveChangesAsync(ct);
        }

        if (hubContext != null)
        {
            try
            {
                await hubContext.Clients.Group($"pipeline_{pipelineId}").SendAsync(
                    "PipelineNodeExecutionUpdated",
                    new { executionId, pipelineId, nodeId, status = "running" },
                    ct
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to broadcast PipelineNodeExecutionUpdated via SignalR for {NodeId}", nodeId);
            }
        }
    }

    private async Task RecordNodeFailureAsync(Guid executionId, Guid pipelineId, Guid nodeId, string error, CancellationToken ct)
    {
        var nodeExec = await db.NodeExecutions
            .FirstOrDefaultAsync(x => x.PipelineExecutionId == executionId && x.PipelineNodeId == nodeId, ct);

        if (nodeExec == null)
        {
            nodeExec = new NodeExecution(executionId, nodeId, status: ExecutionStatus.Running);
            nodeExec.MarkFailed(error);
            db.NodeExecutions.Add(nodeExec);
        }
        else
        {
            nodeExec.MarkFailed(error);
        }

        await db.SaveChangesAsync(ct);

        if (hubContext != null)
        {
            try
            {
                await hubContext.Clients.Group($"pipeline_{pipelineId}").SendAsync(
                    "PipelineNodeExecutionUpdated",
                    new { executionId, pipelineId, nodeId, status = "failed" },
                    ct
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to broadcast PipelineNodeExecutionUpdated via SignalR for {NodeId}", nodeId);
            }
        }
    }
}
