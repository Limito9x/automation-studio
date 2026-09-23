using Automation.Tag.Contracts;
using Automation.Tag.Contracts.Dtos;
using Automation.Workspace.Contracts.Extensions;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.Resources.GetResourceById;

[NonTransactional]
public class GetResourceByIdHandler(WorkspaceDbContext db, ITagApi tagApi)
{
    public async Task<Result<ResourceItemDto>> HandleAsync(GetResourceByIdQuery query, CancellationToken ct)
    {
        var resource = await db.ResourceItems
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Include(x => x.Workspace)
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(ct);

        if (resource is null)
            return Result.Fail($"Resource with ID '{query.Id}' was not found.");

        var versionIds = resource.Versions.Select(v => v.Id).ToList();
        var tagsByVersion = new Dictionary<Guid, IReadOnlyList<TagLinkDetailDto>>();

        if (versionIds.Count > 0)
        {
            var tagResult = await tagApi.GetTagsByEntitiesAsync("ResourceVersion", versionIds, ct);
            if (tagResult.IsSuccess && tagResult.Value != null)
            {
                tagsByVersion = tagResult.Value.ToDictionary(k => k.Key, v => v.Value);
            }
        }

        var versionDtos = resource.Versions
            .OrderByDescending(v => v.VersionNo)
            .Select(v =>
            {
                var links = tagsByVersion.GetValueOrDefault(v.Id) ?? [];
                var tagsByPath = links
                    .GroupBy(t => !string.IsNullOrEmpty(t.TargetSubPath) ? t.TargetSubPath : TagMigrationHelper.ExtractPath(t.MetadataJson))
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .ToDictionary(g => g.Key, g => (IReadOnlyList<TagLinkDetailDto>)g.ToList());

                return new ResourceVersionDto(
                    v.Id,
                    v.VersionNo,
                    v.SizeBytes,
                    v.FileHash,
                    v.Notes,
                    v.CreatedAt,
                    v.Metadata,
                    tagsByPath
                );
            })
            .ToList();

        var dto = new ResourceItemDto(
            resource.Id,
            resource.Workspace?.ProjectId ?? Guid.Empty,
            resource.WorkspaceId,
            resource.DisplayName,
            resource.RelativePath,
            resource.PlatformExtensionId,
            resource.ContentId,
            resource.CreatedAt,
            versionDtos
        );

        return Result.Ok(dto);
    }
}
