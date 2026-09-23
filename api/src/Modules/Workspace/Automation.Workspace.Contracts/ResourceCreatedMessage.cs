namespace Automation.Workspace.Contracts;

public record ResourceVersionCreatedInfo(
    Guid ResourceVersionId,
    Guid PlatformExtensionId,
    Guid? ContentId = null,
    string? Extension = null,
    string? RelativePath = null
);

public record ResourcesCreatedEvent(
    Guid ProjectId,
    Guid WorkspaceId,
    Guid AgentId,
    List<ResourceVersionCreatedInfo> ResourceVersions
)
{
    public List<Guid> ResourceVersionIds => ResourceVersions.Select(r => r.ResourceVersionId).ToList();
}
