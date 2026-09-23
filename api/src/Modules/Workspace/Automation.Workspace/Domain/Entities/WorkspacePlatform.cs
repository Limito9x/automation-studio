namespace Automation.Workspace.Domain.Entities;

public class WorkspacePlatform : AuditableEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;

    public Guid PlatformId { get; set; }

    public WorkspacePlatform() { }

    public WorkspacePlatform(Guid workspaceId, Guid platformId)
    {
        WorkspaceId = workspaceId;
        PlatformId = platformId;
    }
}
