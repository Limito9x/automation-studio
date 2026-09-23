using Automation.Files.Contracts;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Nodes.DeleteCustomNode;

[Transactional(typeof(PipelineDbContext))]
public class DeleteCustomNodeHandler(
    PipelineDbContext db,
    IAssetApi assetApi
)
{
    public async Task<Result> HandleAsync(
        DeleteCustomNodeCommand command,
        CancellationToken ct
    )
    {
        var node = await db.NodeDefinitions
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (node == null)
        {
            return Result.Fail("Node definition not found.");
        }

        db.NodeDefinitions.Remove(node);
        await db.SaveChangesAsync(ct);

        // Remove linked script asset
        await assetApi.RemoveLinkAsync(
            ownerEntityId: node.Id.ToString(),
            ownerEntityType: "NodeDefinition",
            slotKey: PipelineAssetSlots.CustomScript,
            ct: ct
        );

        return Result.Ok();
    }
}
