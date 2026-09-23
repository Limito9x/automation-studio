namespace Automation.Workspace.Domain.Entities;

public class ResourceVersionLocation : AuditableEntity
{
    public Guid ResourceVersionId { get; set; }
    public ResourceVersion ResourceVersion { get; set; } = null!;

    public Guid WorkspaceAgentId { get; set; }
    public WorkspaceAgent WorkspaceAgent { get; set; } = null!;
    public bool IsOrigin { get; set; }
    public DateTimeOffset DiscoveredAt { get; set; }

    public ResourceVersionLocation() { }

    public ResourceVersionLocation(
        Guid resourceVersionId,
        Guid workspaceAgentId,
        bool isOrigin = false,
        DateTimeOffset? discoveredAt = null
    )
    {
        ResourceVersionId = resourceVersionId;
        WorkspaceAgentId = workspaceAgentId;
        IsOrigin = isOrigin;
        DiscoveredAt = discoveredAt ?? DateTimeOffset.UtcNow;
    }

    public void SetOrigin(bool isOrigin)
    {
        IsOrigin = isOrigin;
    }
}
