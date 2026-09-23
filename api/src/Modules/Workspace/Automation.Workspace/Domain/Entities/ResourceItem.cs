namespace Automation.Workspace.Domain.Entities;

public class ResourceItem : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public string DisplayName { get; set; } = string.Empty;
    public Guid PlatformExtensionId { get; set; }

    // Định danh duy nhất trong workspace - không thể thay đổi
    public string RelativePath { get; set; } = string.Empty;
    public Guid? ContentId { get; set; }

    private readonly List<ResourceVersion> _versions = new();
    public IReadOnlyList<ResourceVersion> Versions => _versions.AsReadOnly();

    public ResourceItem() { }

    public ResourceItem(
        Guid workspaceId,
        string displayName,
        Guid platformExtensionId,
        string relativePath,
        Guid? contentId = null
    )
    {
        WorkspaceId = workspaceId;
        DisplayName = displayName;
        PlatformExtensionId = platformExtensionId;
        ContentId = contentId;
        RelativePath = relativePath;
    }

    public static ResourceItem Create(
        Guid workspaceId,
        Guid workspaceAgentId,
        Guid platformExtensionId,
        string name,
        string relativePath,
        string fileHash,
        long sizeBytes = 0,
        string? notes = null
    )
    {
        var resource = new ResourceItem(workspaceId, name, platformExtensionId, relativePath);
        resource.AddNewVersion(workspaceAgentId, fileHash, sizeBytes, notes);
        return resource;
    }

    public ResourceVersion AddNewVersion(
        Guid workspaceAgentId,
        string fileHash,
        long sizeBytes = 0,
        string? notes = null
    )
    {
        var newVersion = new ResourceVersion(Id, _versions.Count + 1, sizeBytes, fileHash, notes);

        newVersion.AddLocation(workspaceAgentId);

        _versions.Add(newVersion);
        return newVersion;
    }

    public ResourceVersion? LatestVersion =>
        _versions.OrderByDescending(x => x.VersionNo).FirstOrDefault();

    public bool HasOnLocal(Guid workspaceAgentId) =>
        _versions.Any(x => x.Locations.Any(y => y.WorkspaceAgentId == workspaceAgentId));

    public void AssignContent(Guid? contentId)
    {
        ContentId = contentId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
