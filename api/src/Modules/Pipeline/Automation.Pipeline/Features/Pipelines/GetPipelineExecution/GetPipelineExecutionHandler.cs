using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.GetPipelineExecution;

public record GetPipelineExecutionQuery(Guid Id);

[NonTransactional]
public class GetPipelineExecutionHandler(PipelineDbContext db)
{
    public async Task<Result<PipelineExecutionDto>> HandleAsync(
        GetPipelineExecutionQuery query,
        CancellationToken ct
    )
    {
        var exec = await db.PipelineExecutions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct);

        if (exec == null)
        {
            return Result.Fail<PipelineExecutionDto>($"Pipeline execution '{query.Id}' not found.");
        }

        var dto = new PipelineExecutionDto(
            exec.Id,
            exec.PipelineId,
            exec.AgentId,
            exec.Status,
            exec.StartedAt,
            exec.FinishedAt,
            exec.ErrorMessage,
            exec.NextNodeIndex,
            exec.CurrentBatchId,
            exec.ExecutionState
        );

        return Result.Ok(dto);
    }
}
