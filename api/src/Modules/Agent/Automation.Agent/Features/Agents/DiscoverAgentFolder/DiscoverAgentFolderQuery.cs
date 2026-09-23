namespace Automation.Agent.Features.Agents.DiscoverAgentFolder;

public record DiscoverAgentFolderQuery(
    Guid Id,
    string? Path = null
);

public record DirectoryNodeDto(
    string Name,
    string Path,
    bool HasChildren = false
);

public record DiscoverAgentFolderResult(
    string CurrentPath,
    string ParentPath,
    bool CanNavigateUp,
    IReadOnlyList<DirectoryNodeDto> Items
);