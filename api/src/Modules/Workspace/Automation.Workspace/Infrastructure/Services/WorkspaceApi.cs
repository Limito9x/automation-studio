using Automation.Tag.Contracts;
using Automation.Tag.Contracts.Dtos;
using Automation.Workspace.Contracts;
using Automation.Workspace.Contracts.Extensions;
using Automation.Workspace.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Automation.Workspace.Infrastructure.Services;

public class WorkspaceApi(WorkspaceDbContext db, IMessageBus bus, ITagApi tagApi) : IWorkspaceApi
{
    public async Task<Result<ResourceLocationInfoDto>> GetResourceLocationAsync(
        Guid resourceVersionId,
        CancellationToken ct = default
    )
    {
        var version = await db
            .ResourceVersions.AsNoTracking()
            .Where(v => v.Id == resourceVersionId)
            .Include(v => v.Resource)
            .Include(v => v.Locations)
            .FirstOrDefaultAsync(ct);

        // Fallback: If not found by ResourceVersionId, check if it's a ResourceId and get the latest version
        if (version == null)
        {
            version = await db
                .ResourceVersions.AsNoTracking()
                .Where(v => v.ResourceId == resourceVersionId)
                .Include(v => v.Resource)
                .Include(v => v.Locations)
                .OrderByDescending(v => v.VersionNo)
                .FirstOrDefaultAsync(ct);
        }

        if (version == null)
            return Result.Fail("Resource version not found.");

        Guid? agentId = null;
        string? rootPath = null;

        var originLocation =
            version.Locations.FirstOrDefault(l => l.IsOrigin)
            ?? (version.Locations.Count > 0 ? version.Locations[0] : null);
        if (originLocation != null)
        {
            var wsAgent = await db
                .WorkspaceAgents.AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == originLocation.WorkspaceAgentId, ct);

            if (wsAgent != null)
            {
                agentId = wsAgent.AgentId;
                rootPath = wsAgent.RootPath;
            }
        }

        return Result.Ok(
            new ResourceLocationInfoDto(
                version.Id,
                version.ResourceId,
                version.Resource?.RelativePath ?? string.Empty,
                version.FileHash,
                agentId,
                rootPath,
                version.Resource?.ContentId
            )
        );
    }

    public async Task<
        Result<Dictionary<string, ResourceLocationInfoDto>>
    > GetResourceLocationsAsync(
        IEnumerable<Guid> resourceVersionIds,
        Guid agentId,
        CancellationToken ct = default
    )
    {
        var idsList = resourceVersionIds.ToList();
        var resourceLocations = await db
            .ResourceVersionLocations.Where(l =>
                idsList.Contains(l.ResourceVersionId) && l.WorkspaceAgent.AgentId == agentId
            )
            .Include(l => l.WorkspaceAgent)
            .Include(l => l.ResourceVersion)
                .ThenInclude(v => v.Resource)
            .ToDictionaryAsync(k => k.ResourceVersion.Id.ToString(), ct);

        var result = new Dictionary<string, ResourceLocationInfoDto>();
        foreach (var loc in resourceLocations)
        {
            var dto = new ResourceLocationInfoDto(
                loc.Value.ResourceVersionId,
                loc.Value.ResourceVersion.ResourceId,
                loc.Value.ResourceVersion.Resource?.RelativePath ?? string.Empty,
                loc.Value.ResourceVersion.FileHash,
                loc.Value.WorkspaceAgent.AgentId,
                loc.Value.WorkspaceAgent.RootPath,
                loc.Value.ResourceVersion.Resource?.ContentId
            );
            result[loc.Key] = dto;
            result[loc.Value.ResourceVersion.ResourceId.ToString()] = dto;
        }

        // Fallback for any missing IDs: check if they are ResourceIds
        var missingIds = idsList.Where(id => !result.ContainsKey(id.ToString())).ToList();
        if (missingIds.Count > 0)
        {
            var fallbackVersions = await db
                .ResourceVersions.AsNoTracking()
                .Where(v => missingIds.Contains(v.ResourceId))
                .Include(v => v.Resource)
                .Include(v => v.Locations)
                    .ThenInclude(l => l.WorkspaceAgent)
                .OrderByDescending(v => v.VersionNo)
                .ToListAsync(ct);

            foreach (var group in fallbackVersions.GroupBy(v => v.ResourceId))
            {
                var latest = group.First();
                var loc =
                    latest.Locations.FirstOrDefault(l => l.WorkspaceAgent?.AgentId == agentId)
                    ?? latest.Locations.FirstOrDefault(l => l.IsOrigin)
                    ?? latest.Locations.FirstOrDefault();

                var dto = new ResourceLocationInfoDto(
                    latest.Id,
                    latest.ResourceId,
                    latest.Resource?.RelativePath ?? string.Empty,
                    latest.FileHash,
                    loc?.WorkspaceAgent?.AgentId,
                    loc?.WorkspaceAgent?.RootPath,
                    latest.Resource?.ContentId
                );
                result[group.Key.ToString()] = dto;
                result[latest.Id.ToString()] = dto;
            }
        }

        if (result.Count == 0)
            return Result.Fail("Locations not found.");

        return Result.Ok(result);
    }

    public async Task<Result<SyncLocalChangesResultDto>> SyncLocalChangesAsync(
        Guid workspaceId,
        Guid agentId,
        List<string> targetPaths,
        string? notes = null,
        CancellationToken ct = default
    )
    {
        var cmd = new Features.WorkspaceAgents.SyncLocalChanges.SyncLocalChangesCommand(
            workspaceId,
            agentId,
            notes,
            targetPaths,
            null
        );

        var result = await bus.InvokeAsync<
            Result<Features.WorkspaceAgents.SyncLocalChanges.SyncLocalChangesResult>
        >(cmd, ct);
        if (result.IsFailed)
            return Result.Fail(result.Errors);

        return Result.Ok(
            new SyncLocalChangesResultDto(
                result.Value.WorkspaceId,
                result.Value.AgentId,
                result.Value.AddedCount,
                result.Value.ModifiedCount,
                result.Value.LocationRemove,
                result.Value.ResourceVersionIds ?? [],
                result.Value.SyncedResources ?? []
            )
        );
    }

    public async Task<Result<List<Guid>>> GetUncoveredWorkspacesAsync(
        Guid agentId,
        IEnumerable<Guid> requiredWorkspaceIds,
        CancellationToken ct = default
    )
    {
        var requiredList = requiredWorkspaceIds.Distinct().ToList();
        if (requiredList.Count == 0)
            return Result.Ok(new List<Guid>());

        var coveredWorkspaceIds = await db
            .WorkspaceAgents.AsNoTracking()
            .Where(w => w.AgentId == agentId && requiredList.Contains(w.WorkspaceId))
            .Select(w => w.WorkspaceId)
            .Distinct()
            .ToListAsync(ct);

        var uncovered = requiredList.Except(coveredWorkspaceIds).ToList();
        return Result.Ok(uncovered);
    }

    public async Task<Result<Dictionary<Guid, string>>> GetWorkspaceNamesAsync(
        IEnumerable<Guid> workspaceIds,
        CancellationToken ct = default
    )
    {
        var idList = workspaceIds.Distinct().ToList();
        if (idList.Count == 0)
            return Result.Ok(new Dictionary<Guid, string>());

        var dict = await db
            .Workspaces.AsNoTracking()
            .Where(w => idList.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.Name, ct);

        return Result.Ok(dict);
    }

    public async Task<Result<string>> GetWorkspaceRootPathAsync(
        Guid workspaceId,
        Guid agentId,
        CancellationToken ct = default
    )
    {
        var wsAgent = await db
            .WorkspaceAgents.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WorkspaceId == workspaceId && w.AgentId == agentId, ct);

        if (wsAgent == null)
            return Result.Fail<string>(
                $"Workspace '{workspaceId}' is not assigned to Agent '{agentId}'."
            );

        return Result.Ok(wsAgent.RootPath);
    }

    public async Task<Result> UpdateMetadataAsync(
        Guid resourceVersionId,
        System.Text.Json.JsonDocument? metadata,
        CancellationToken ct = default
    )
    {
        var version = await db.ResourceVersions.FirstOrDefaultAsync(
            v => v.Id == resourceVersionId,
            ct
        );

        if (version == null)
            return Result.Fail($"ResourceVersion with ID '{resourceVersionId}' not found.");

        version.SetMetadata(metadata);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<System.Text.Json.JsonDocument?>> GetMetadataAsync(
        Guid resourceVersionId,
        CancellationToken ct = default
    )
    {
        var version = await db
            .ResourceVersions.AsNoTracking()
            .Where(v => v.Id == resourceVersionId)
            .Select(v => v.Metadata)
            .FirstOrDefaultAsync(ct);

        if (version == null)
        {
            version = await db
                .ResourceVersions.AsNoTracking()
                .Where(v => v.ResourceId == resourceVersionId)
                .OrderByDescending(v => v.VersionNo)
                .Select(v => v.Metadata)
                .FirstOrDefaultAsync(ct);
        }

        return Result.Ok(version);
    }

    public async Task<
        Result<Contracts.Dtos.ResourceMetadataDetailDto>
    > GetMetadataDetailWithTagsAsync(Guid resourceVersionId, CancellationToken ct = default)
    {
        var version = await db
            .ResourceVersions.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == resourceVersionId, ct);

        if (version == null)
        {
            version = await db
                .ResourceVersions.AsNoTracking()
                .Where(v => v.ResourceId == resourceVersionId)
                .OrderByDescending(v => v.VersionNo)
                .FirstOrDefaultAsync(ct);
        }

        if (version == null)
            return Result.Fail("Resource version not found.");

        var tagResult = await tagApi.GetTagsByEntityAsync("ResourceVersion", version.Id, ct);

        var tagsByPath =
            tagResult.IsSuccess && tagResult.Value != null
                ? tagResult
                    .Value.GroupBy(t =>
                        !string.IsNullOrEmpty(t.TargetSubPath)
                            ? t.TargetSubPath
                            : TagMigrationHelper.ExtractPath(t.MetadataJson)
                    )
                    .ToDictionary(g => g.Key, g => (IReadOnlyList<TagLinkDetailDto>)g.ToList())
                : new Dictionary<string, IReadOnlyList<TagLinkDetailDto>>();

        return Result.Ok(
            new Contracts.Dtos.ResourceMetadataDetailDto(version.Id, version.Metadata, tagsByPath)
        );
    }

    public async Task<
        Result<Dictionary<Guid, Contracts.Dtos.ResourceBatchItemDto>>
    > GetBatchResourceDetailsAsync(
        IEnumerable<Guid> resourceOrVersionIds,
        CancellationToken ct = default
    )
    {
        var idList = resourceOrVersionIds.Distinct().ToList();
        if (idList.Count == 0)
            return Result.Ok(new Dictionary<Guid, Contracts.Dtos.ResourceBatchItemDto>());

        // 1. Query ResourceVersions matching IDs directly
        var versionsById = await db
            .ResourceVersions.AsNoTracking()
            .Where(v => idList.Contains(v.Id))
            .Include(v => v.Resource)
            .Include(v => v.Locations)
                .ThenInclude(l => l.WorkspaceAgent)
            .ToListAsync(ct);

        // 2. For remaining IDs, check if they are ResourceIds and pick their latest version
        var foundVersionIds = versionsById.Select(v => v.Id).ToHashSet();
        var foundResourceIds = versionsById.Select(v => v.ResourceId).ToHashSet();
        var remainingIds = idList
            .Where(id => !foundVersionIds.Contains(id) && !foundResourceIds.Contains(id))
            .ToList();

        if (remainingIds.Count > 0)
        {
            var versionsByResourceId = await db
                .ResourceVersions.AsNoTracking()
                .Where(v => remainingIds.Contains(v.ResourceId))
                .Include(v => v.Resource)
                .Include(v => v.Locations)
                    .ThenInclude(l => l.WorkspaceAgent)
                .OrderByDescending(v => v.VersionNo)
                .ToListAsync(ct);

            foreach (var group in versionsByResourceId.GroupBy(v => v.ResourceId))
            {
                versionsById.Add(group.First());
            }
        }

        if (versionsById.Count == 0)
            return Result.Ok(new Dictionary<Guid, Contracts.Dtos.ResourceBatchItemDto>());

        var allVersionIds = versionsById.Select(v => v.Id).Distinct().ToList();
        var allResourceIds = versionsById.Select(v => v.ResourceId).Distinct().ToList();

        // 3. Batch query tags
        var versionTagsResult = await tagApi.GetTagsByEntitiesAsync(
            "ResourceVersion",
            allVersionIds,
            ct
        );
        var versionTagsMap =
            versionTagsResult.IsSuccess && versionTagsResult.Value != null
                ? versionTagsResult.Value
                : new Dictionary<Guid, IReadOnlyList<TagLinkDetailDto>>();

        var resourceTagsResult = await tagApi.GetTagsByEntitiesAsync(
            "Resource",
            allResourceIds,
            ct
        );
        var resourceTagsMap =
            resourceTagsResult.IsSuccess && resourceTagsResult.Value != null
                ? resourceTagsResult.Value
                : new Dictionary<Guid, IReadOnlyList<TagLinkDetailDto>>();

        // 4. Build output dictionary
        var result = new Dictionary<Guid, Contracts.Dtos.ResourceBatchItemDto>();

        foreach (var v in versionsById)
        {
            var resName =
                v.Resource != null && !string.IsNullOrWhiteSpace(v.Resource.DisplayName)
                    ? v.Resource.DisplayName
                    : (
                        !string.IsNullOrWhiteSpace(v.Resource?.RelativePath)
                            ? Path.GetFileNameWithoutExtension(v.Resource.RelativePath)
                            : v.Id.ToString()
                    );

            var relPath = v.Resource?.RelativePath;
            string? fullPath = null;
            var originLoc =
                v.Locations.FirstOrDefault(l => l.IsOrigin)
                ?? (v.Locations.Count > 0 ? v.Locations[0] : null);
            if (
                originLoc != null
                && !string.IsNullOrWhiteSpace(originLoc.WorkspaceAgent?.RootPath)
                && !string.IsNullOrWhiteSpace(relPath)
            )
            {
                var cleanRel = relPath.TrimStart('/', '\\');
                fullPath = Path.Combine(originLoc.WorkspaceAgent.RootPath, cleanRel)
                    .Replace('\\', '/');
            }
            else
            {
                fullPath = relPath;
            }

            var versionTags = versionTagsMap.TryGetValue(v.Id, out var vTags)
                ? vTags
                : Array.Empty<TagLinkDetailDto>();
            var tagMap = versionTags
                .GroupBy(t =>
                    !string.IsNullOrEmpty(t.TargetSubPath)
                        ? t.TargetSubPath
                        : TagMigrationHelper.ExtractPath(t.MetadataJson)
                )
                .ToDictionary(g => g.Key, g => (IReadOnlyList<TagLinkDetailDto>)g.ToList());

            var resourceTags = resourceTagsMap.TryGetValue(v.ResourceId, out var rTags)
                ? rTags
                : Array.Empty<TagLinkDetailDto>();

            var itemDto = new Contracts.Dtos.ResourceBatchItemDto(
                v.ResourceId,
                v.Id,
                resName,
                relPath,
                fullPath,
                v.Metadata,
                resourceTags,
                tagMap
            );

            result[v.Id] = itemDto;
            result[v.ResourceId] = itemDto;
        }

        return Result.Ok(result);
    }

    public async Task<Result> AssignResourceToContentAsync(
        Guid ContentId,
        List<Guid> resourceIds,
        CancellationToken ct = default
    )
    {
        if (resourceIds.Count == 0)
            return Result.Ok();

        // 1. Tìm các ResourceId trực tiếp khớp với ResourceItem.Id
        var directIds = await db
            .ResourceItems.Where(r => resourceIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(ct);

        // 2. Với các ID còn lại, kiểm tra nếu là ResourceVersionId để lấy ResourceId tương ứng
        var remainingIds = resourceIds.Except(directIds).ToList();
        if (remainingIds.Count > 0)
        {
            var fromVersionIds = await db
                .ResourceVersions.Where(v => remainingIds.Contains(v.Id))
                .Select(v => v.ResourceId)
                .Distinct()
                .ToListAsync(ct);
            directIds.AddRange(fromVersionIds);
        }

        if (directIds.Count > 0)
        {
            var distinctIds = directIds.Distinct().ToList();
            await db
                .ResourceItems.Where(r => distinctIds.Contains(r.Id))
                .ExecuteUpdateAsync(
                    s => s.SetProperty(r => r.ContentId, ContentId)
                          .SetProperty(r => r.UpdatedAt, DateTimeOffset.UtcNow),
                    ct
                );
        }

        return Result.Ok();
    }
}

