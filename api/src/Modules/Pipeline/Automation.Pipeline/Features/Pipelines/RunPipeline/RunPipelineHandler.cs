using System.Text.Json;
using Automation.Files.Contracts;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Engine;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Workspace.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.RunPipeline;

[NonTransactional]
public class RunPipelineHandler(
    PipelineDbContext db,
    IMessageBus messageBus,
    IWorkspaceApi workspaceApi,
    IAssetApi assetApi
)
{
    public async Task<Result<PipelineExecutionDto>> HandleAsync(
        RunPipelineCommand command,
        CancellationToken ct
    )
    {
        var pipeline = await db.Pipelines
            .Include(x => x.Nodes)
            .Include(x => x.Inputs)
            .FirstOrDefaultAsync(x => x.Id == command.PipelineId, ct);

        if (pipeline == null)
        {
            return Result.Fail<PipelineExecutionDto>($"Pipeline '{command.PipelineId}' not found.");
        }

        var agentId = command.AgentId;
        if (agentId == Guid.Empty)
        {
            agentId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        // Validate required Start Inputs
        var requiredMissing = pipeline.Inputs
            .Where(i => i.IsRequired && i.DefaultValue == null)
            .Where(i => command.RuntimeInputs == null ||
                        (!command.RuntimeInputs.ContainsKey(i.Key) && !command.RuntimeInputs.ContainsKey(i.Label)))
            .ToList();

        if (requiredMissing.Count > 0)
        {
            var missingLabels = string.Join(", ", requiredMissing.Select(i => $"'{i.Label}' ({i.Key})"));
            return Result.Fail<PipelineExecutionDto>($"Missing required pipeline start input(s): {missingLabels}.");
        }

        // 1. Collect required workspaces from pipeline nodes (e.g. SyncLocalChange or Workspace pins)
        var requiredWorkspaceIds = new HashSet<Guid>();
        foreach (var node in pipeline.Nodes)
        {
            if (node.Config != null)
            {
                try
                {
                    var root = node.Config.RootElement;
                    if (root.TryGetProperty("WorkspaceId", out var wElem))
                    {
                        var str = wElem.GetString();
                        if (Guid.TryParse(str, out var wGuid) && wGuid != Guid.Empty)
                        {
                            requiredWorkspaceIds.Add(wGuid);
                        }
                    }
                }
                catch
                {
                    // Ignore JSON parse errors for non-object configs
                }
            }
        }

        // 2. Validate Agent Coverage for required workspaces (only if external agent tools are required)
        if (requiredWorkspaceIds.Count > 0 && agentId != Guid.Parse("00000000-0000-0000-0000-000000000001"))
        {
            var uncoveredResult = await workspaceApi.GetUncoveredWorkspacesAsync(agentId, requiredWorkspaceIds, ct);
            if (uncoveredResult.IsFailed)
            {
                return Result.Fail<PipelineExecutionDto>(uncoveredResult.Errors);
            }

            var uncovered = uncoveredResult.Value;
            if (uncovered.Count > 0)
            {
                var namesResult = await workspaceApi.GetWorkspaceNamesAsync(uncovered, ct);
                var names = namesResult.IsSuccess && namesResult.Value.Count > 0
                    ? string.Join(", ", namesResult.Value.Values.Select(n => $"'{n}'"))
                    : string.Join(", ", uncovered);

                return Result.Fail<PipelineExecutionDto>(
                    $"Selected Agent is not assigned to required workspace(s): {names}. Please add WorkspaceAgent before running this pipeline."
                );
            }
        }

        // 3. Create Execution record and pre-save initial RuntimeInputs in ExecutionState
        var execution = new PipelineExecution(pipeline.Id, agentId);

        var initialState = new Automation.Pipeline.Engine.Models.PipelineExecutionState();
        if (command.RuntimeInputs != null)
        {
            foreach (var (k, v) in command.RuntimeInputs)
            {
                initialState.RuntimeInputs[k] = v;
            }
        }
        execution.SetState(initialState.ToJsonDocument(), 0);

        db.PipelineExecutions.Add(execution);
        await db.SaveChangesAsync(ct);

        // Link any uploaded runtime input assets so they don't get cleaned up
        if (command.RuntimeInputs != null && command.RuntimeInputs.Count > 0)
        {
            await PipelineAssetHelper.LinkRuntimeInputAssetsAsync(assetApi, execution.Id, command.RuntimeInputs, null, ct);
        }

        // 4. Trigger Execution Engine asynchronously (Direct in-process invocation)
        var executionId = execution.Id;
        _ = Task.Run(async () =>
        {
            try
            {
                await messageBus.InvokeAsync(new TriggerPipelineExecutionMessage(executionId));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Pipeline Execution {executionId} Error] {ex.Message}");
            }
        });

        var dto = new PipelineExecutionDto(
            execution.Id,
            execution.PipelineId,
            execution.AgentId,
            execution.Status,
            execution.StartedAt,
            execution.FinishedAt,
            execution.ErrorMessage,
            execution.NextNodeIndex,
            execution.CurrentBatchId,
            execution.ExecutionState
        );

        return Result.Ok(dto);
    }
}
