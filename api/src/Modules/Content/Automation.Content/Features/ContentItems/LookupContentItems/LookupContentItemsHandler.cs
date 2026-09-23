using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Content.Features.ContentItems.LookupContentItems;

[NonTransactional]
public class LookupContentItemsHandler(ContentDbContext db)
{
    public async Task<Result<List<ContentLookupDto>>> HandleAsync(
        LookupContentItemsQuery query,
        CancellationToken ct)
    {
        var queryable = db.ContentItems
            .AsNoTracking()
            .Where(c => c.ProjectId == query.ProjectId);

        if (query.ContentTypeId.HasValue)
        {
            queryable = queryable.Where(c => c.ContentTypeId == query.ContentTypeId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(query.ContentTypeKey))
        {
            queryable = queryable.Where(c => c.ContentType.Key == query.ContentTypeKey);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            queryable = queryable.Where(c => EF.Functions.ILike(c.Name, $"%{kw}%"));
        }

        var limit = query.Limit <= 0 ? 50 : Math.Min(query.Limit, 100);

        var items = await queryable
            .OrderBy(c => c.Name)
            .Take(limit)
            .Select(c => new ContentLookupDto(
                c.Id,
                c.Name,
                c.ContentTypeId,
                c.ContentType.Key,
                c.ContentType.Name,
                c.ContentType.Color,
                c.ContentType.Icon
            ))
            .ToListAsync(ct);

        return Result.Ok(items);
    }
}
