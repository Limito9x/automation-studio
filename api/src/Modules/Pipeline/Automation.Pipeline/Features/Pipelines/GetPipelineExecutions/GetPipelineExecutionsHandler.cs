using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.GetPipelineExecutions;

[NonTransactional]
public class GetPipelineExecutionsHandler(PipelineDbContext db)
{
    public async Task<Result<List<PipelineExecutionDto>>> HandleAsync(
        GetPipelineExecutionsQuery query,
        CancellationToken ct
    )
    {
        var executions = await db.PipelineExecutions
            .AsNoTracking()
            .Where(x => x.PipelineId == query.PipelineId)
            .OrderByDescending(x => x.StartedAt)
            .Take(50)
            .Select(x => new PipelineExecutionDto(
                x.Id,
                x.PipelineId,
                x.AgentId,
                x.Status,
                x.StartedAt,
                x.FinishedAt,
                x.ErrorMessage,
                x.NextNodeIndex,
                x.CurrentBatchId,
                x.ExecutionState
            ))
            .ToListAsync(ct);

        return Result.Ok(executions);
    }
}
