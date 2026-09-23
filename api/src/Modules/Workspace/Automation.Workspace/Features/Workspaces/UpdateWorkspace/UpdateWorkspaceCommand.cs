namespace Automation.Workspace.Features.Workspaces.UpdateWorkspace;

public record UpdateWorkspaceCommand(
    Guid Id,
    string Name,
    List<Guid>? PlatformIds = null
);

public class UpdateWorkspaceRequest
{
    public string Name { get; set; } = string.Empty;
    public List<Guid>? PlatformIds { get; set; }
}
