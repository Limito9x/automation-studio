using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Workspace.Features.ResourceVersions.GetResourceVersions;

public class GetResourceVersionsHandler(WorkspaceDbContext db)
{
    public async Task<Result<IReadOnlyList<ResourceVersionDto>>> HandleAsync(GetResourceVersionsQuery query, CancellationToken ct)
    {
        var versions = await db.ResourceVersions
            .AsNoTracking()
            .Where(x => x.ResourceId == query.ResourceId)
            .OrderByDescending(x => x.VersionNo)
            .ProjectToType<ResourceVersionDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<ResourceVersionDto>>(versions);
    }
}

