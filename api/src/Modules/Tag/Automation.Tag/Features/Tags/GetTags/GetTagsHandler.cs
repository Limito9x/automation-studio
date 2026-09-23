using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Tag.Features.Tags.GetTags;

[NonTransactional]
public class GetTagsHandler(TagDbContext db)
{
    public async Task<Result<IReadOnlyList<TagItemDto>>> HandleAsync(GetTagsQuery query, CancellationToken ct)
    {
        var tags = db.TagItems.AsNoTracking();

        if (query.ProjectId.HasValue)
            tags = tags.Where(x => x.ProjectId == query.ProjectId.Value);

        if (!string.IsNullOrWhiteSpace(query.RootPath))
            tags = tags.Where(x => x.Path.IsDescendantOf(query.RootPath));

        if (!string.IsNullOrWhiteSpace(query.Search))
            tags = tags.Where(x => x.Name.Contains(query.Search) || ((string)x.Path).Contains(query.Search));

        var result = await tags
            .OrderBy(x => x.Path)
            .Select(x => new TagItemDto(
                x.Id,
                x.ProjectId,
                x.Path,
                x.Name,
                x.Color,
                x.Description,
                x.ParentId,
                x.CreatedAt
            ))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<TagItemDto>>(result);
    }
}