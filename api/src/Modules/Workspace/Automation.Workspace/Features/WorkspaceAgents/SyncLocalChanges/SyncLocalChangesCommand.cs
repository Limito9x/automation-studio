namespace Automation.Workspace.Features.WorkspaceAgents.SyncLocalChanges;

public record SyncLocalChangesCommand(
    Guid WorkspaceId,
    Guid AgentId,
    string? Notes,
    List<string> TargetPaths,
    Dictionary<string, string>? NewResourceNames
);

public record SyncLocalChangesResult(
    Guid WorkspaceId,
    Guid AgentId,
    int AddedCount,
    int ModifiedCount,
    int LocationRemove,
    List<Guid> ResourceVersionIds,
    Dictionary<string, Guid> SyncedResources
);
