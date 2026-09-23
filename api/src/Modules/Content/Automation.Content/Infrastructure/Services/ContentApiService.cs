using Automation.Content.Contracts;
using Automation.Content.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Infrastructure.Services;

public class ContentApiService(ContentDbContext db) : IContentApi
{
    public async Task<Result<IReadOnlyDictionary<Guid, ContentSummaryDto>>> GetContentsByProjectIdAsync(
        Guid projectId, 
        CancellationToken ct = default)
    {
        var items = await db.ContentItems
            .AsNoTracking()
            .Where(c => c.ProjectId == projectId)
            .Select(c => new ContentSummaryDto(
                c.Id,
                c.Name,
                c.ContentTypeId,
                c.ContentType.Name,
                c.ContentType.Color,
                c.ContentType.Icon
            ))
            .ToListAsync(ct);

        var map = items.ToDictionary(x => x.Id);
        return Result.Ok<IReadOnlyDictionary<Guid, ContentSummaryDto>>(map);
    }

    public async Task<Result<ContentSummaryDto>> GetContentByIdAsync(
        Guid contentId, 
        CancellationToken ct = default)
    {
        var item = await db.ContentItems
            .AsNoTracking()
            .Where(c => c.Id == contentId)
            .Select(c => new ContentSummaryDto(
                c.Id,
                c.Name,
                c.ContentTypeId,
                c.ContentType.Name,
                c.ContentType.Color,
                c.ContentType.Icon
            ))
            .FirstOrDefaultAsync(ct);

        if (item == null)
        {
            return Result.Fail<ContentSummaryDto>($"Content item with ID '{contentId}' not found.");
        }

        return Result.Ok(item);
    }

    public async Task<Result<IReadOnlyDictionary<Guid, ContentSummaryDto>>> GetContentsByIdsAsync(
        IEnumerable<Guid> contentIds, 
        CancellationToken ct = default)
    {
        var idsList = contentIds.Distinct().ToList();
        var items = await db.ContentItems
            .AsNoTracking()
            .Where(c => idsList.Contains(c.Id))
            .Select(c => new ContentSummaryDto(
                c.Id,
                c.Name,
                c.ContentTypeId,
                c.ContentType.Name,
                c.ContentType.Color,
                c.ContentType.Icon
            ))
            .ToListAsync(ct);

        var map = items.ToDictionary(x => x.Id);
        return Result.Ok<IReadOnlyDictionary<Guid, ContentSummaryDto>>(map);
    }
}
