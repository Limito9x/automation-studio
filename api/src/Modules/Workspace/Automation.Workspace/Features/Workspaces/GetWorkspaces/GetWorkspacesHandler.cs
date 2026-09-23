using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.Workspaces.GetWorkspaces;

[NonTransactional]
public class GetWorkspacesHandler(WorkspaceDbContext db)
{
    public async Task<Result<IReadOnlyList<WorkspaceDto>>> HandleAsync(GetWorkspacesQuery query, CancellationToken ct)
    {
        var workspaces = await db.Workspaces
            .AsNoTracking()
            .Where(x => x.ProjectId == query.ProjectId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WorkspaceDto(
                x.Id,
                x.ProjectId,
                x.Name,
                x.WorkspaceAgents.Count,
                x.Resources.Count,
                x.CreatedAt,
                x.WorkspacePlatforms.Select(wp => wp.PlatformId).ToList()
            ))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<WorkspaceDto>>(workspaces);
    }
}
