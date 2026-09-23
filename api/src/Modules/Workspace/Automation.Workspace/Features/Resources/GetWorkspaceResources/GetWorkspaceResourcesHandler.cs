using Automation.Content.Contracts;
using Automation.SharedKernel.Abstractions.Querying;
using Automation.SharedKernel.Infrastructure.Querying;
using Automation.Workspace.Domain.Entities;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using FluentResults;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.Resources.GetWorkspaceResources;

[NonTransactional]
public class GetWorkspaceResourcesHandler(WorkspaceDbContext db, IContentApi contentApi)
{
    public async Task<Result<PagedResult<WorkspaceResourceDto>>> HandleAsync(
        GetWorkspaceResourcesQuery query,
        CancellationToken ct
    )
    {
        var baseQuery = db
            .ResourceItems.AsNoTracking()
            .Include(r => r.Versions)
            .Where(r => r.WorkspaceId == query.WorkspaceId);

        // 1. Lấy Map Content của Project (Single-pass)
        var contentMap = new Dictionary<Guid, ContentSummaryDto>();
        var contentResult = await contentApi.GetContentsByProjectIdAsync(query.ProjectId, ct);
        if (contentResult.IsSuccess && contentResult.Value != null)
        {
            contentMap = new Dictionary<Guid, ContentSummaryDto>(contentResult.Value);
        }

        // 2. Xử lý Omni-Search nếu có từ khóa
        if (!string.IsNullOrWhiteSpace(query.GlobalKeyword))
        {
            var kw = query.GlobalKeyword.Trim();
            var matchedContentIds = contentMap
                .Where(c =>
                    c.Value.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)
                    || c.Value.ContentTypeName.Contains(kw, StringComparison.OrdinalIgnoreCase)
                )
                .Select(c => c.Key)
                .ToList();

            baseQuery = baseQuery.Where(r =>
                EF.Functions.ILike(r.DisplayName, $"%{kw}%")
                || EF.Functions.ILike(r.RelativePath, $"%{kw}%")
                || (r.ContentId != null && matchedContentIds.Contains(r.ContentId.Value))
            );
        }

        // 3. Gridify Phân trang & Sắp xếp trên SQL
        var mapper = new GridifyMapper<ResourceItem>()
            .GenerateMappings()
            .AddMap("displayName", r => r.DisplayName)
            .AddMap("relativePath", r => r.RelativePath)
            .AddMap("createdAt", r => r.CreatedAt);

        var pagedResult = await baseQuery.ToPagedResultAsync(query, mapper, ct);
        if (pagedResult.IsFailed)
        {
            return pagedResult.ToResult();
        }

        // 4. Enrich dữ liệu Content từ Map vào DTO
        var dtos = pagedResult
            .Value.Items.Select(r => new WorkspaceResourceDto(
                Id: r.Id,
                WorkspaceId: r.WorkspaceId,
                DisplayName: r.DisplayName,
                RelativePath: r.RelativePath,
                PlatformExtensionId: r.PlatformExtensionId,
                ContentId: r.ContentId,
                ContentName: r.ContentId != null
                && contentMap.TryGetValue(r.ContentId.Value, out var c)
                    ? c.Name
                    : null,
                ContentTypeName: r.ContentId != null
                && contentMap.TryGetValue(r.ContentId.Value, out var c2)
                    ? c2.ContentTypeName
                    : null,
                ContentTypeColor: r.ContentId != null
                && contentMap.TryGetValue(r.ContentId.Value, out var c3)
                    ? c3.ContentTypeColor
                    : null,
                ContentTypeIcon: r.ContentId != null
                && contentMap.TryGetValue(r.ContentId.Value, out var c4)
                    ? c4.ContentTypeIcon
                    : null,
                VersionCount: r.Versions.Count,
                CreatedAt: r.CreatedAt,
                LatestVersionId: r.Versions.OrderByDescending(v => v.VersionNo).Select(v => (Guid?)v.Id).FirstOrDefault(),
                LatestVersionNo: r.Versions.OrderByDescending(v => v.VersionNo).Select(v => (int?)v.VersionNo).FirstOrDefault()
            ))
            .ToList();

        return Result.Ok(
            PagedResult<WorkspaceResourceDto>.From(
                dtos,
                pagedResult.Value.TotalCount,
                pagedResult.Value.Page,
                pagedResult.Value.PageSize
            )
        );
    }
}
