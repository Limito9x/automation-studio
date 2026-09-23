using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.Resources.GetResourcesByContent;

[NonTransactional]
public class GetResourcesByContentHandler(WorkspaceDbContext db)
{
    public async Task<Result<List<ContentResourceDto>>> HandleAsync(
        GetResourcesByContentQuery query,
        CancellationToken ct
    )
    {
        var resources = await db.ResourceItems
            .AsNoTracking()
            .Include(r => r.Workspace)
            .Include(r => r.Versions)
            .Where(r => r.ContentId == query.ContentId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        var dtos = resources.Select(r =>
        {
            var latest = r.LatestVersion;
            return new ContentResourceDto(
                Id: r.Id,
                WorkspaceId: r.WorkspaceId,
                WorkspaceName: r.Workspace?.Name ?? string.Empty,
                DisplayName: r.DisplayName,
                RelativePath: r.RelativePath,
                PlatformExtensionId: r.PlatformExtensionId,
                LatestVersionNo: latest?.VersionNo ?? 0,
                LatestSizeBytes: latest?.SizeBytes ?? 0,
                VersionCount: r.Versions.Count,
                CreatedAt: r.CreatedAt
            );
        }).ToList();

        return Result.Ok(dtos);
    }
}
