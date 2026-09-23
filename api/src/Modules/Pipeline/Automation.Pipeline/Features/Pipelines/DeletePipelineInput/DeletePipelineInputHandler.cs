using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.DeletePipelineInput;

[Transactional(typeof(PipelineDbContext))]
public class DeletePipelineInputHandler(PipelineDbContext db)
{
    public async Task<Result> HandleAsync(
        DeletePipelineInputCommand command,
        CancellationToken ct
    )
    {
        var input = await db.PipelineInputs
            .FirstOrDefaultAsync(x => x.Id == command.InputId && x.PipelineId == command.PipelineId, ct);

        if (input == null)
        {
            return Result.Fail($"Pipeline input '{command.InputId}' not found.");
        }

        // Remove any outgoing edges from Start node with this source pin key
        var edges = await db.PipelineEdges
            .Where(e => e.PipelineId == command.PipelineId && e.SourcePin == input.Key)
            .ToListAsync(ct);

        if (edges.Count > 0)
        {
            db.PipelineEdges.RemoveRange(edges);
        }

        db.PipelineInputs.Remove(input);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
