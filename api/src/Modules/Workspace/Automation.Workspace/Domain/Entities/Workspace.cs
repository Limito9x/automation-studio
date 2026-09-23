namespace Automation.Workspace.Domain.Entities;

public class Workspace : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<ResourceItem> Resources { get; set; } = new List<ResourceItem>();
    public ICollection<WorkspaceAgent> WorkspaceAgents { get; set; } = new List<WorkspaceAgent>();
    public ICollection<WorkspacePlatform> WorkspacePlatforms { get; set; } = new List<WorkspacePlatform>();

    public Workspace() { }

    public Workspace(Guid projectId, string name)
    {
        ProjectId = projectId;
        Name = name;
    }

    public void Update(string name)
    {
        Name = name;
    }

    public void AddPlatform(Guid platformId)
    {
        if (!WorkspacePlatforms.Any(x => x.PlatformId == platformId))
        {
            WorkspacePlatforms.Add(new WorkspacePlatform(Id, platformId));
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void RemovePlatform(Guid platformId)
    {
        var wp = WorkspacePlatforms.FirstOrDefault(x => x.PlatformId == platformId);
        if (wp is not null)
        {
            WorkspacePlatforms.Remove(wp);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}

