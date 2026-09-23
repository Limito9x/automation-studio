namespace Automation.Workspace.Features.Workspaces.CreateWorkspace;

public record CreateWorkspaceCommand(
    Guid ProjectId,
    string Name,
    List<Guid>? PlatformIds = null
);
