using FluentResults;

namespace Automation.Workspace.Contracts;

public interface IWorkspaceApi
{
    Task<Result<ResourceLocationInfoDto>> GetResourceLocationAsync(
        Guid resourceVersionId,
        CancellationToken ct = default
    );

    Task<Result<Dictionary<string, ResourceLocationInfoDto>>> GetResourceLocationsAsync(
        IEnumerable<Guid> resourceVersionIds,
        Guid agentId, // Chỉ định rõ máy nào để lấy tài nguyên, tránh mơ hồ
        CancellationToken ct = default
    );

    Task<Result<SyncLocalChangesResultDto>> SyncLocalChangesAsync(
        Guid workspaceId,
        Guid agentId,
        List<string> targetPaths,
        string? notes = null,
        CancellationToken ct = default
    );

    Task<Result<List<Guid>>> GetUncoveredWorkspacesAsync(
        Guid agentId,
        IEnumerable<Guid> requiredWorkspaceIds,
        CancellationToken ct = default
    );

    Task<Result<Dictionary<Guid, string>>> GetWorkspaceNamesAsync(
        IEnumerable<Guid> workspaceIds,
        CancellationToken ct = default
    );

    Task<Result<string>> GetWorkspaceRootPathAsync(
        Guid workspaceId,
        Guid agentId,
        CancellationToken ct = default
    );

    Task<Result> UpdateMetadataAsync(
        Guid resourceVersionId,
        System.Text.Json.JsonDocument? metadata,
        CancellationToken ct = default
    );

    Task<Result<System.Text.Json.JsonDocument?>> GetMetadataAsync(
        Guid resourceVersionId,
        CancellationToken ct = default
    );

    Task<Result<Dtos.ResourceMetadataDetailDto>> GetMetadataDetailWithTagsAsync(
        Guid resourceVersionId,
        CancellationToken ct = default
    );

    Task<Result<Dictionary<Guid, Dtos.ResourceBatchItemDto>>> GetBatchResourceDetailsAsync(
        IEnumerable<Guid> resourceOrVersionIds,
        CancellationToken ct = default
    );

    Task<Result> AssignResourceToContentAsync(
        Guid ContentId,
        List<Guid> resourceIds,
        CancellationToken ct = default
    );
}

public record SyncLocalChangesResultDto(
    Guid WorkspaceId,
    Guid AgentId,
    int AddedCount,
    int ModifiedCount,
    int LocationRemoved,
    List<Guid> ResourceVersionIds,
    Dictionary<string, Guid> SyncedResources
);

public record ResourceLocationInfoDto(
    Guid ResourceVersionId,
    Guid ResourceId,
    string RelativePath,
    string? FileHash,
    Guid? AgentId,
    string? AgentRootPath,
    Guid? ContentId = null
)
{
    public string? FullLocalPath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(RelativePath))
                return null;

            if (!string.IsNullOrWhiteSpace(AgentRootPath))
            {
                var cleanRel = RelativePath.TrimStart('/', '\\');
                return Path.Combine(AgentRootPath, cleanRel).Replace('\\', '/');
            }

            return RelativePath;
        }
    }
}
