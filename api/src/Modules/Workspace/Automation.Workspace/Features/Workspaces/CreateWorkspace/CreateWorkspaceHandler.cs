using Automation.Workspace.Domain.Entities;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.Workspaces.CreateWorkspace;

[Transactional(typeof(WorkspaceDbContext))]
public class CreateWorkspaceHandler(WorkspaceDbContext db)
{
    public async Task<Result<WorkspaceDto>> HandleAsync(CreateWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = new Domain.Entities.Workspace(
            command.ProjectId,
            command.Name
        );

        db.Workspaces.Add(workspace);

        if (command.PlatformIds != null && command.PlatformIds.Count > 0)
        {
            var platforms = command.PlatformIds.Distinct()
                .Select(pId => new WorkspacePlatform(workspace.Id, pId))
                .ToList();
            db.WorkspacePlatforms.AddRange(platforms);
        }

        await db.SaveChangesAsync(ct);

        var dto = new WorkspaceDto(
            workspace.Id,
            workspace.ProjectId,
            workspace.Name,
            0,
            0,
            workspace.CreatedAt,
            command.PlatformIds ?? []
        );

        return Result.Ok(dto);
    }
}
