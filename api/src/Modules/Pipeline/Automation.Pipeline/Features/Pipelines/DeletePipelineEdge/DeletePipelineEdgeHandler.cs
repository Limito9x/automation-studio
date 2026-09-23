using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.DeletePipelineEdge;

[Transactional(typeof(PipelineDbContext))]
public class DeletePipelineEdgeHandler(PipelineDbContext db)
{
    public async Task<Result> HandleAsync(
        DeletePipelineEdgeCommand command,
        CancellationToken ct
    )
    {
        var edge = await db.PipelineEdges
            .FirstOrDefaultAsync(x => x.Id == command.EdgeId && x.PipelineId == command.PipelineId, ct);

        if (edge == null)
        {
            return Result.Fail($"Edge '{command.EdgeId}' not found in Pipeline '{command.PipelineId}'.");
        }

        db.PipelineEdges.Remove(edge);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
