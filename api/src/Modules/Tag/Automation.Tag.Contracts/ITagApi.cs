using Automation.Tag.Contracts.Dtos;
using FluentResults;

namespace Automation.Tag.Contracts;

/// <summary>
/// Public API for other modules to query and manipulate tags.
/// </summary>
public interface ITagApi
{
    /// <summary>Ensure tag path exists (auto-creates parent tags if needed). Returns the leaf tag.</summary>
    Task<Result<TagDto>> EnsureTagAsync(
        Guid projectId,
        string tagPath,
        string? color = null,
        string? description = null,
        CancellationToken ct = default
    );

    /// <summary>Assign a tag path to an entity, auto-creating tag path nodes if needed.</summary>
    Task<Result<TagLinkDto>> AssignTagAsync(
        Guid projectId,
        string entityType,
        Guid entityId,
        string tagPath,
        string? targetSubPath = null,
        string? metadataJson = null,
        CancellationToken ct = default
    );

    /// <summary>Remove a tag from an entity.</summary>
    Task<Result> RemoveTagAsync(
        Guid projectId,
        string entityType,
        Guid entityId,
        string tagPath,
        string? targetSubPath = null,
        CancellationToken ct = default
    );

    /// <summary>Get all tag links for a single entity.</summary>
    Task<Result<IReadOnlyList<TagLinkDetailDto>>> GetTagsByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken ct = default
    );

    /// <summary>Get tags for multiple entities of the same type (bulk).</summary>
    Task<Result<IReadOnlyDictionary<Guid, IReadOnlyList<TagLinkDetailDto>>>> GetTagsByEntitiesAsync(
        string entityType,
        IEnumerable<Guid> entityIds,
        CancellationToken ct = default
    );

    /// <summary>Query entity IDs that match a tag path (optionally including child tags via ltree).</summary>
    Task<Result<IReadOnlyList<Guid>>> GetEntityIdsByTagQueryAsync(
        Guid projectId,
        string entityType,
        string tagPathQuery,
        bool includeDescendants = true,
        CancellationToken ct = default
    );

    /// <summary>Get all tags in a project, optionally filtered by root prefix.</summary>
    Task<Result<IReadOnlyList<TagDto>>> GetTagsByProjectAsync(
        Guid projectId,
        string? rootPath = null,
        CancellationToken ct = default
    );

    /// <summary>Get tags by their IDs.</summary>
    Task<Result<IReadOnlyDictionary<Guid, TagDto>>> GetTagsAsync(
        IReadOnlyList<Guid> tagIds,
        CancellationToken ct = default
    );

    /// <summary>Update metadata JSON of tag links.</summary>
    Task<Result> UpdateTagLinksMetadataAsync(
        IReadOnlyDictionary<Guid, string> tagLinkIdToMetadataJson,
        CancellationToken ct = default
    );
}
