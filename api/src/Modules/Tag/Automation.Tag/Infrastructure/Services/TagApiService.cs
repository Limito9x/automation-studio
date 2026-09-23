using System.Text.Json;
using System.Text.RegularExpressions;
using Automation.Tag.Contracts;
using Automation.Tag.Contracts.Dtos;
using Automation.Tag.Domain.Entities;
using Automation.Tag.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Automation.Tag.Infrastructure.Services;

public partial class TagApiService(TagDbContext db) : ITagApi
{
    [GeneratedRegex(@"^[A-Za-z0-9_]+(\.[A-Za-z0-9_]+)*$")]
    private static partial Regex LtreePathRegex();

    public async Task<Result<TagDto>> EnsureTagAsync(
        Guid projectId,
        string tagPath,
        string? color = null,
        string? description = null,
        CancellationToken ct = default
    )
    {
        var normalizedPath = NormalizeTagPath(tagPath);
        if (string.IsNullOrWhiteSpace(normalizedPath))
            return Result.Fail<TagDto>("Tag path cannot be empty.");

        if (!LtreePathRegex().IsMatch(normalizedPath))
            return Result.Fail<TagDto>($"Invalid tag path format: '{tagPath}'. Each segment must contain only alphanumeric characters and underscores.");

        var segments = normalizedPath.Split('.');
        TagItem? parentNode = null;
        var currentPrefix = "";

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            currentPrefix = i == 0 ? segment : $"{currentPrefix}.{segment}";
            var isLeaf = i == segments.Length - 1;

            var existingNode = await db.TagItems
                .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Path == currentPrefix, ct);

            if (existingNode == null)
            {
                existingNode = new TagItem(
                    projectId,
                    currentPrefix,
                    segment,
                    isLeaf ? color : null,
                    isLeaf ? description : null,
                    parentNode?.Id
                );
                db.TagItems.Add(existingNode);
                await db.SaveChangesAsync(ct);
            }
            else if (isLeaf && (color != null || description != null))
            {
                if (color != null) existingNode.Color = color;
                if (description != null) existingNode.Description = description;
                await db.SaveChangesAsync(ct);
            }

            parentNode = existingNode;
        }

        return Result.Ok(new TagDto(
            parentNode!.Id,
            parentNode.ProjectId,
            parentNode.Path,
            parentNode.Name,
            parentNode.Color,
            parentNode.Description,
            parentNode.CreatedAt
        ));
    }

    public async Task<Result<TagLinkDto>> AssignTagAsync(
        Guid projectId,
        string entityType,
        Guid entityId,
        string tagPath,
        string? targetSubPath = null,
        string? metadataJson = null,
        CancellationToken ct = default
    )
    {
        var tagResult = await EnsureTagAsync(projectId, tagPath, ct: ct);
        if (tagResult.IsFailed)
            return Result.Fail<TagLinkDto>(tagResult.Errors);

        var tagDto = tagResult.Value;

        var existingLink = await db.TagLinks
            .FirstOrDefaultAsync(
                l => l.ProjectId == projectId &&
                     l.EntityType == entityType &&
                     l.EntityId == entityId &&
                     l.TagId == tagDto.Id &&
                     l.TargetSubPath == targetSubPath,
                ct
            );

        JsonDocument? metaDoc = null;
        if (!string.IsNullOrWhiteSpace(metadataJson))
        {
            try
            {
                metaDoc = JsonDocument.Parse(metadataJson);
            }
            catch (Exception ex)
            {
                return Result.Fail<TagLinkDto>($"Invalid metadata JSON: {ex.Message}");
            }
        }

        if (existingLink != null)
        {
            if (metaDoc != null)
            {
                existingLink.Metadata = metaDoc;
                await db.SaveChangesAsync(ct);
            }

            return Result.Ok(new TagLinkDto(
                existingLink.Id,
                existingLink.ProjectId,
                existingLink.TagId,
                existingLink.TagPath,
                existingLink.EntityType,
                existingLink.EntityId,
                existingLink.TargetSubPath,
                existingLink.Metadata?.RootElement.ToString(),
                tagDto
            ));
        }

        var newLink = new TagLink(
            projectId,
            tagDto.Id,
            tagDto.Path,
            entityType,
            entityId,
            targetSubPath,
            metaDoc
        );

        db.TagLinks.Add(newLink);
        await db.SaveChangesAsync(ct);

        return Result.Ok(new TagLinkDto(
            newLink.Id,
            newLink.ProjectId,
            newLink.TagId,
            newLink.TagPath,
            newLink.EntityType,
            newLink.EntityId,
            newLink.TargetSubPath,
            newLink.Metadata?.RootElement.ToString(),
            tagDto
        ));
    }

    public async Task<Result> RemoveTagAsync(
        Guid projectId,
        string entityType,
        Guid entityId,
        string tagPath,
        string? targetSubPath = null,
        CancellationToken ct = default
    )
    {
        var normalized = NormalizeTagPath(tagPath);
        var query = db.TagLinks.Where(
            l => l.ProjectId == projectId &&
                 l.EntityType == entityType &&
                 l.EntityId == entityId &&
                 l.TagPath == normalized
        );

        if (targetSubPath != null)
        {
            query = query.Where(l => l.TargetSubPath == targetSubPath);
        }

        var links = await query.ToListAsync(ct);
        if (links.Count > 0)
        {
            db.TagLinks.RemoveRange(links);
            await db.SaveChangesAsync(ct);
        }

        return Result.Ok();
    }

    public async Task<Result<IReadOnlyList<TagLinkDetailDto>>> GetTagsByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken ct = default
    )
    {
        var links = await db.TagLinks.AsNoTracking()
            .Include(l => l.Tag)
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderBy(l => l.TagPath)
            .ToListAsync(ct);

        var result = links.Select(l => new TagLinkDetailDto(
            l.Id,
            l.TagId,
            l.TagPath,
            l.Tag.Name,
            l.Tag.Color,
            l.Tag.Description,
            l.TargetSubPath,
            l.Metadata?.RootElement.ToString()
        )).ToList();

        return Result.Ok<IReadOnlyList<TagLinkDetailDto>>(result);
    }

    public async Task<Result<IReadOnlyDictionary<Guid, IReadOnlyList<TagLinkDetailDto>>>> GetTagsByEntitiesAsync(
        string entityType,
        IEnumerable<Guid> entityIds,
        CancellationToken ct = default
    )
    {
        var ids = entityIds.ToHashSet();
        if (ids.Count == 0)
        {
            return Result.Ok<IReadOnlyDictionary<Guid, IReadOnlyList<TagLinkDetailDto>>>(
                new Dictionary<Guid, IReadOnlyList<TagLinkDetailDto>>()
            );
        }

        var links = await db.TagLinks.AsNoTracking()
            .Include(l => l.Tag)
            .Where(l => l.EntityType == entityType && ids.Contains(l.EntityId))
            .OrderBy(l => l.TagPath)
            .ToListAsync(ct);

        var grouped = links
            .GroupBy(l => l.EntityId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<TagLinkDetailDto>)g.Select(l => new TagLinkDetailDto(
                    l.Id,
                    l.TagId,
                    l.TagPath,
                    l.Tag.Name,
                    l.Tag.Color,
                    l.Tag.Description,
                    l.TargetSubPath,
                    l.Metadata?.RootElement.ToString()
                )).ToList()
            );

        return Result.Ok<IReadOnlyDictionary<Guid, IReadOnlyList<TagLinkDetailDto>>>(grouped);
    }

    public async Task<Result<IReadOnlyList<Guid>>> GetEntityIdsByTagQueryAsync(
        Guid projectId,
        string entityType,
        string tagPathQuery,
        bool includeDescendants = true,
        CancellationToken ct = default
    )
    {
        var normalized = NormalizeTagPath(tagPathQuery);
        var query = db.TagLinks.AsNoTracking()
            .Where(l => l.ProjectId == projectId && l.EntityType == entityType);

        if (includeDescendants)
        {
            // Match exact or child node via ltree descendant operator (<@)
            query = query.Where(l => l.TagPath.IsDescendantOf(normalized));
        }
        else
        {
            query = query.Where(l => l.TagPath == normalized);
        }

        var entityIds = await query.Select(l => l.EntityId).Distinct().ToListAsync(ct);
        return Result.Ok<IReadOnlyList<Guid>>(entityIds);
    }

    public async Task<Result<IReadOnlyList<TagDto>>> GetTagsByProjectAsync(
        Guid projectId,
        string? rootPath = null,
        CancellationToken ct = default
    )
    {
        var query = db.TagItems.AsNoTracking()
            .Where(t => t.ProjectId == projectId);

        if (!string.IsNullOrWhiteSpace(rootPath))
        {
            var normalized = NormalizeTagPath(rootPath);
            query = query.Where(t => t.Path.IsDescendantOf(normalized));
        }

        var tags = await query
            .OrderBy(t => t.Path)
            .Select(t => new TagDto(
                t.Id,
                t.ProjectId,
                t.Path,
                t.Name,
                t.Color,
                t.Description,
                t.CreatedAt
            ))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<TagDto>>(tags);
    }

    public async Task<Result<IReadOnlyDictionary<Guid, TagDto>>> GetTagsAsync(
        IReadOnlyList<Guid> tagIds,
        CancellationToken ct = default
    )
    {
        var tags = await db.TagItems.AsNoTracking()
            .Where(t => tagIds.Contains(t.Id))
            .ToDictionaryAsync(
                t => t.Id,
                t => new TagDto(
                    t.Id,
                    t.ProjectId,
                    t.Path,
                    t.Name,
                    t.Color,
                    t.Description,
                    t.CreatedAt
                ),
                ct
            );

        return Result.Ok<IReadOnlyDictionary<Guid, TagDto>>(tags);
    }

    public async Task<Result> UpdateTagLinksMetadataAsync(
        IReadOnlyDictionary<Guid, string> tagLinkIdToMetadataJson,
        CancellationToken ct = default
    )
    {
        if (tagLinkIdToMetadataJson.Count == 0)
            return Result.Ok();

        var linkIds = tagLinkIdToMetadataJson.Keys.ToList();
        var links = await db.TagLinks
            .Where(x => linkIds.Contains(x.Id))
            .ToListAsync(ct);

        foreach (var link in links)
        {
            if (tagLinkIdToMetadataJson.TryGetValue(link.Id, out var jsonStr))
            {
                var doc = string.IsNullOrWhiteSpace(jsonStr) ? null : JsonDocument.Parse(jsonStr);
                link.Metadata = doc;
            }
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private static string NormalizeTagPath(string path)
    {
        return path.Trim().Trim('.').Replace('/', '.').Replace('\\', '.');
    }
}
