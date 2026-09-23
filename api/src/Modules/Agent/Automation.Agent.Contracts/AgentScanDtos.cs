namespace Automation.Agent.Contracts;

public record AgentScanItemDto(
    string RelativePath,
    string Hash,
    long SizeBytes
);

public record AgentScanResultDto(
    string CommandId,
    bool Success,
    string? ErrorMessage,
    IReadOnlyList<AgentScanItemDto>? Items
);

public record AgentBrowseItemDto(
    string Name,
    string Path,
    bool IsDirectory,
    long SizeBytes
);

public record AgentBrowseResultDto(
    string CommandId,
    bool Success,
    string? ErrorMessage,
    string CurrentPath,
    string ParentPath,
    bool CanNavigateUp,
    IReadOnlyList<AgentBrowseItemDto>? Items
);
