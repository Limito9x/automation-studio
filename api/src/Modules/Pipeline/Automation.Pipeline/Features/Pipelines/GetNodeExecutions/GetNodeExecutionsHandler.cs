using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.GetNodeExecutions;

public record GetNodeExecutionsQuery(Guid ExecutionId);

[NonTransactional]
public class GetNodeExecutionsHandler(PipelineDbContext db)
{
    public async Task<Result<List<NodeExecutionDto>>> HandleAsync(
        GetNodeExecutionsQuery query,
        CancellationToken ct
    )
    {
        var nodeExecs = await db.NodeExecutions
            .AsNoTracking()
            .Where(x => x.PipelineExecutionId == query.ExecutionId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

        var dtos = nodeExecs.Select(x => new NodeExecutionDto(
            x.Id,
            x.PipelineExecutionId,
            x.PipelineNodeId,
            x.Status,
            x.StartedAt,
            x.FinishedAt,
            x.ErrorMessage,
            x.Output,
            x.Log
        )).ToList();

        return Result.Ok(dtos);
    }
}
