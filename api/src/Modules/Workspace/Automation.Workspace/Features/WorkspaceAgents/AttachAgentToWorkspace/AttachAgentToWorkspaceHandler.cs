using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.WorkspaceAgents.AttachAgentToWorkspace;

[Transactional(typeof(WorkspaceDbContext))]
public class AttachAgentToWorkspaceHandler(WorkspaceDbContext db)
{
    public async Task<Result<WorkspaceAgentDto>> HandleAsync(AttachAgentToWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = await db.Workspaces.FindAsync([command.WorkspaceId], ct);
        if (workspace is null)
            return Result.Fail($"Workspace with ID '{command.WorkspaceId}' was not found.");

        var existingAgent = await db.WorkspaceAgents
            .FirstOrDefaultAsync(x => x.WorkspaceId == command.WorkspaceId && x.AgentId == command.AgentId, ct);

        if (existingAgent is not null)
        {
            existingAgent.UpdateRootPath(command.RootPath);
            await db.SaveChangesAsync(ct);
            return Result.Ok(existingAgent.Adapt<WorkspaceAgentDto>());
        }

        var workspaceAgent = new Domain.Entities.WorkspaceAgent(
            command.WorkspaceId,
            command.AgentId,
            command.RootPath
        );

        db.WorkspaceAgents.Add(workspaceAgent);
        await db.SaveChangesAsync(ct);

        return Result.Ok(workspaceAgent.Adapt<WorkspaceAgentDto>());
    }
}
