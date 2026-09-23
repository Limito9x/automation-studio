using System.Text.Json;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Engine.DataResolver;
using Automation.Pipeline.Engine.ExecPlanner;
using Automation.Pipeline.Engine.Models;
using Automation.Pipeline.Engine.Orchestrator.Dispatchers;
using Automation.Pipeline.Hubs;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Pipeline.Tools;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Engine.Orchestrator;

public class PipelineOrchestrator(
    PipelineDbContext db,
    IExecPlanner execPlanner,
    IExecutionMemoryStore memoryStore,
    DotNetSegmentDispatcher dotNetDispatcher,
    AgentSegmentDispatcher agentDispatcher,
    ForEachDispatcher forEachDispatcher,
    IToolRegistry toolRegistry,
    IHubContext<PipelineExecutionHub>? hubContext,
    ILogger<PipelineOrchestrator> logger
) : IPipelineOrchestrator
{
    public async Task<Result<PipelineExecution>> ExecuteOrResumeAsync(
        Guid executionId,
        Dictionary<string, object?>? runtimeInputs = null,
        CancellationToken ct = default
    )
    {
        var execution = await db.PipelineExecutions
            .Include(x => x.Pipeline)
                .ThenInclude(p => p.Nodes)
            .Include(x => x.Pipeline)
                .ThenInclude(p => p.Edges)
            .Include(x => x.Pipeline)
                .ThenInclude(p => p.Inputs)
            .FirstOrDefaultAsync(x => x.Id == executionId, ct);

        if (execution == null)
        {
            return Result.Fail<PipelineExecution>($"Pipeline execution '{executionId}' not found.");
        }

        if (execution.Status == ExecutionStatus.Succeeded || execution.Status == ExecutionStatus.Cancelled)
        {
            return Result.Ok(execution);
        }

        // 1. Load Custom Node Definitions for this Project
        var customDefs = await db.NodeDefinitions
            .AsNoTracking()
            .Where(x => x.ProjectId == execution.Pipeline.ProjectId)
            .ToListAsync(ct);

        // 2. Populate Runtime / Start Inputs into Memory Store (from defaults, persisted ExecutionState, or arguments)
        var mergedStartInputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (execution.Pipeline.Inputs != null)
        {
            foreach (var input in execution.Pipeline.Inputs)
            {
                if (!string.IsNullOrEmpty(input.DefaultValue))
                {
                    try
                    {
                        var parsedVal = JsonSerializer.Deserialize<object>(input.DefaultValue);
                        mergedStartInputs[input.Key] = parsedVal;
                    }
                    catch
                    {
                        mergedStartInputs[input.Key] = input.DefaultValue;
                    }
                }
            }
        }

        if (execution.ExecutionState != null)
        {
            try
            {
                var stateObj = Automation.Pipeline.Engine.Models.PipelineExecutionState.FromJsonDocument(execution.ExecutionState);
                if (stateObj?.RuntimeInputs != null)
                {
                    foreach (var (k, v) in stateObj.RuntimeInputs)
                    {
                        mergedStartInputs[k] = v;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to parse RuntimeInputs from ExecutionState for {ExecutionId}", execution.Id);
            }
        }

        if (runtimeInputs != null)
        {
            foreach (var (k, v) in runtimeInputs)
            {
                mergedStartInputs[k] = v;
            }
        }

        foreach (var (k, v) in mergedStartInputs)
        {
            await memoryStore.SetStartInputAsync(execution.Id, k, v, ct);
        }

        // 3. Initialize Pipeline Variables into Memory Store (Execution Context) only when starting execution
        if (execution.NextNodeIndex == 0 && execution.Pipeline.Variables != null)
        {
            foreach (var v in execution.Pipeline.Variables)
            {
                var existing = await memoryStore.GetVariableAsync(execution.Id, v.Name, ct);
                if (existing == null)
                {
                    object? initVal = v.Cardinality switch
                    {
                        PinCardinality.Map => new Dictionary<string, object?>(),
                        PinCardinality.Array => new List<object?>(),
                        _ => v.Type switch
                        {
                            PinPrimitiveType.Number => 0,
                            PinPrimitiveType.Boolean => false,
                            _ => string.Empty
                        }
                    };
                    await memoryStore.SetVariableAsync(execution.Id, v.Name, initVal, ct);
                }
            }
        }

        // 4. Compile ExecPlan
        var plan = execPlanner.BuildExecPlan(
            execution.Pipeline,
            customDefs,
            toolRegistry,
            runtimeInputs
        );

        if (!plan.IsValid)
        {
            if (plan.CycleNodeIds.Count > 0)
            {
                var cycleError = $"Pipeline contains cycle involving nodes: {string.Join(", ", plan.CycleNodeIds)}";
                execution.MarkFailed(cycleError, execution.ExecutionState ?? JsonDocument.Parse("{}"));
                await db.SaveChangesAsync(ct);
                return Result.Fail<PipelineExecution>(cycleError);
            }

            if (plan.UnresolvedPins.Count > 0)
            {
                return Result.Fail<PipelineExecution>(new UnresolvedPinsError(plan.UnresolvedPins));
            }
        }

        if (execution.Status == ExecutionStatus.Pending)
        {
            execution.Start();
            await db.SaveChangesAsync(ct);

            if (hubContext != null)
            {
                await hubContext.Clients.Group($"pipeline_{execution.PipelineId}").SendAsync(
                    "PipelineExecutionStarted",
                    new { executionId = execution.Id, pipelineId = execution.PipelineId, status = (int)execution.Status, startedAt = execution.StartedAt },
                    ct
                );
            }
        }
        else if (execution.Status == ExecutionStatus.WaitingForAgent)
        {
            execution.Resume();
            await db.SaveChangesAsync(ct);
        }

        var rootScope = new ScopeContext("root");

        // 5. Sequential Segment Dispatch Loop
        for (var segIdx = execution.NextNodeIndex; segIdx < plan.Segments.Count; segIdx++)
        {
            var segment = plan.Segments[segIdx];
            logger.LogInformation("Processing ExecSegment #{Index} [{Executor}] (Steps: {Count})",
                segIdx, segment.Executor, segment.Steps.Count);

            if (segment.IsFlowControl)
            {
                var fcRes = await forEachDispatcher.DispatchAsync(execution, segment, rootScope, this, ct);
                if (fcRes.IsFailed)
                {
                    var err = fcRes.Errors.FirstOrDefault()?.Message ?? "FlowControl execution failed";
                    execution.MarkFailed(err);
                    await db.SaveChangesAsync(ct);

                    if (hubContext != null)
                    {
                        await hubContext.Clients.Group($"pipeline_{execution.PipelineId}").SendAsync(
                            "PipelineExecutionFinished",
                            new { executionId = execution.Id, pipelineId = execution.PipelineId, status = (int)execution.Status, finishedAt = execution.FinishedAt, errorMessage = err },
                            ct
                        );
                    }

                    return Result.Fail<PipelineExecution>(fcRes.Errors);
                }
            }
            else if (string.Equals(segment.Executor, "dotNet", StringComparison.OrdinalIgnoreCase))
            {
                var dotNetRes = await dotNetDispatcher.DispatchAsync(execution, segment, rootScope, this, ct);
                if (dotNetRes.IsFailed)
                {
                    var err = dotNetRes.Errors.FirstOrDefault()?.Message ?? "DotNet segment execution failed";
                    execution.MarkFailed(err);
                    await db.SaveChangesAsync(ct);

                    if (hubContext != null)
                    {
                        await hubContext.Clients.Group($"pipeline_{execution.PipelineId}").SendAsync(
                            "PipelineExecutionFinished",
                            new { executionId = execution.Id, pipelineId = execution.PipelineId, status = (int)execution.Status, finishedAt = execution.FinishedAt, errorMessage = err },
                            ct
                        );
                    }

                    return Result.Fail<PipelineExecution>(dotNetRes.Errors);
                }
            }
            else
            {
                // Agent Segment (Blender / Unreal / Python)
                var agentRes = await agentDispatcher.DispatchAsync(
                    execution,
                    segment,
                    segIdx + 1,
                    customDefs,
                    rootScope,
                    ct
                );

                if (agentRes.IsFailed)
                {
                    var err = agentRes.Errors.FirstOrDefault()?.Message ?? "Agent dispatch failed";
                    execution.MarkFailed(err);
                    await db.SaveChangesAsync(ct);

                    if (hubContext != null)
                    {
                        await hubContext.Clients.Group($"pipeline_{execution.PipelineId}").SendAsync(
                            "PipelineExecutionFinished",
                            new { executionId = execution.Id, pipelineId = execution.PipelineId, status = (int)execution.Status, finishedAt = execution.FinishedAt, errorMessage = err },
                            ct
                        );
                    }

                    return Result.Fail<PipelineExecution>(agentRes.Errors);
                }

                // Suspend and wait for Agent completion event
                await db.SaveChangesAsync(ct);
                return Result.Ok(execution);
            }
        }

        // 6. All segments succeeded
        execution.MarkSucceeded(execution.ExecutionState ?? JsonDocument.Parse("{}"));
        await db.SaveChangesAsync(ct);

        if (hubContext != null)
        {
            await hubContext.Clients.Group($"pipeline_{execution.PipelineId}").SendAsync(
                "PipelineExecutionFinished",
                new { executionId = execution.Id, pipelineId = execution.PipelineId, status = (int)execution.Status, finishedAt = execution.FinishedAt },
                ct
            );
        }

        logger.LogInformation("Pipeline Execution [{ExecutionId}] completed successfully.", execution.Id);
        return Result.Ok(execution);
    }
}
