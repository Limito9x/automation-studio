using Automation.Workspace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.Resources.AssignResourcesContent;

[Transactional(typeof(WorkspaceDbContext))]
public class AssignResourcesContentHandler(WorkspaceDbContext db)
{
    public async Task<Result> HandleAsync(
        AssignResourcesContentCommand command,
        CancellationToken ct
    )
    {
        if (command.ResourceIds == null || command.ResourceIds.Count == 0)
        {
            return Result.Ok();
        }

        var resources = await db.ResourceItems
            .Where(r => command.ResourceIds.Contains(r.Id))
            .ToListAsync(ct);

        if (resources.Count == 0)
        {
            return Result.Fail("No resources found matching the specified IDs.");
        }

        foreach (var resource in resources)
        {
            resource.AssignContent(command.ContentId);
        }

        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
