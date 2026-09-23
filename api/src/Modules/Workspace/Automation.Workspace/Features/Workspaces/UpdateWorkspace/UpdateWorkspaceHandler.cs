using Automation.Workspace.Domain.Entities;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.Workspaces.UpdateWorkspace;

[Transactional(typeof(WorkspaceDbContext))]
public class UpdateWorkspaceHandler(WorkspaceDbContext db)
{
    public async Task<Result<WorkspaceDto>> HandleAsync(UpdateWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = await db.Workspaces
            .Include(w => w.WorkspaceAgents)
            .Include(w => w.Resources)
            .FirstOrDefaultAsync(w => w.Id == command.Id, ct);

        if (workspace is null)
            return Result.Fail($"Workspace with ID '{command.Id}' was not found.");

        workspace.Update(command.Name);

        var existingPlatforms = await db.WorkspacePlatforms
            .Where(wp => wp.WorkspaceId == command.Id)
            .ToListAsync(ct);

        var platformIdsResult = new List<Guid>();

        if (command.PlatformIds != null)
        {
            var incomingPlatformIds = command.PlatformIds.Distinct().ToHashSet();
            platformIdsResult = incomingPlatformIds.ToList();

            // Xóa các platform bị bỏ
            var toRemove = existingPlatforms.Where(ep => !incomingPlatformIds.Contains(ep.PlatformId)).ToList();
            if (toRemove.Count > 0)
            {
                db.WorkspacePlatforms.RemoveRange(toRemove);
            }

            // Thêm các platform mới
            var existingIds = existingPlatforms.Select(ep => ep.PlatformId).ToHashSet();
            var toAdd = incomingPlatformIds
                .Where(pId => !existingIds.Contains(pId))
                .Select(pId => new WorkspacePlatform(command.Id, pId))
                .ToList();

            if (toAdd.Count > 0)
            {
                db.WorkspacePlatforms.AddRange(toAdd);
            }
        }
        else
        {
            platformIdsResult = existingPlatforms.Select(ep => ep.PlatformId).ToList();
        }

        await db.SaveChangesAsync(ct);

        var dto = new WorkspaceDto(
            workspace.Id,
            workspace.ProjectId,
            workspace.Name,
            workspace.WorkspaceAgents.Count,
            workspace.Resources.Count,
            workspace.CreatedAt,
            platformIdsResult
        );

        return Result.Ok(dto);
    }
}
