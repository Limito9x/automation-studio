using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.GetPipelines;

[NonTransactional]
public class GetPipelinesHandler(PipelineDbContext db)
{
    public async Task<Result<List<PipelineSummaryDto>>> HandleAsync(
        GetPipelinesQuery query,
        CancellationToken ct
    )
    {
        var q = db.Pipelines.AsNoTracking();

        if (query.ProjectId.HasValue && query.ProjectId.Value != Guid.Empty)
        {
            q = q.Where(x => x.ProjectId == query.ProjectId.Value);
        }

        var list = await q
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PipelineSummaryDto(
                x.Id,
                x.ProjectId,
                x.Name,
                x.TriggerType,
                x.TriggerWorkspaceId,
                x.Nodes.Count,
                x.Edges.Count,
                x.CreatedAt,
                x.TriggerConfig
            ))
            .ToListAsync(ct);

        return Result.Ok(list);
    }
}
