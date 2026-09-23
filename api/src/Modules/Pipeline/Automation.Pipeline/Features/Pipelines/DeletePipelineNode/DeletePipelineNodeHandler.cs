using Automation.Files.Contracts;
using Automation.Pipeline.Engine;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.DeletePipelineNode;

[Transactional(typeof(PipelineDbContext))]
public class DeletePipelineNodeHandler(
    PipelineDbContext db,
    IAssetApi assetApi
)
{
    public async Task<Result> HandleAsync(
        DeletePipelineNodeCommand command,
        CancellationToken ct
    )
    {
        var node = await db.PipelineNodes
            .FirstOrDefaultAsync(x => x.Id == command.NodeId && x.PipelineId == command.PipelineId, ct);

        if (node == null)
        {
            return Result.Fail($"Node '{command.NodeId}' not found in Pipeline '{command.PipelineId}'.");
        }

        if (string.Equals(node.Kind, Constants.PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(node.RefId, "Start", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Fail("The Start node is the entry point of the pipeline and cannot be deleted.");
        }

        if (string.Equals(node.Kind, Constants.PipelineNodeKind.Return, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(node.RefId, "Return", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Fail("The Return node is the exit point of the pipeline and cannot be deleted.");
        }

        // Remove any connecting edges first
        var edges = await db.PipelineEdges
            .Where(e => e.PipelineId == command.PipelineId &&
                        (e.SourcePipelineNodeId == command.NodeId || e.TargetPipelineNodeId == command.NodeId))
            .ToListAsync(ct);

        if (edges.Count > 0)
        {
            db.PipelineEdges.RemoveRange(edges);
        }

        // Clean up linked assets for this node
        await PipelineAssetHelper.RemoveNodeAssetsAsync(assetApi, node.Id, node.Config, null, ct);

        db.PipelineNodes.Remove(node);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
