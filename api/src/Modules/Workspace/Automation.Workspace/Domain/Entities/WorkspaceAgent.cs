namespace Automation.Workspace.Domain.Entities;

public class WorkspaceAgent : AuditableEntity
{
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public Guid AgentId { get; set; }
    public string RootPath { get; set; } = string.Empty;

    public ICollection<ResourceVersionLocation> Locations { get; set; } = new List<ResourceVersionLocation>();

    public WorkspaceAgent() { }

    public WorkspaceAgent(Guid workspaceId, Guid agentId, string rootPath)
    {
        WorkspaceId = workspaceId;
        AgentId = agentId;
        RootPath = rootPath;
    }

    public void UpdateRootPath(string rootPath)
    {
        RootPath = rootPath;
    }
}
