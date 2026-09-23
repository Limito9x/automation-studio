using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Tag.Features.Tags.UpdateTag;

[Transactional(typeof(TagDbContext))]
public class UpdateTagHandler(TagDbContext db)
{
    public async Task<Result<TagItemDto>> HandleAsync(UpdateTagCommand command, CancellationToken ct)
    {
        var tag = await db.TagItems.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (tag is null)
            return Result.Fail<TagItemDto>("Tag not found.");

        if (!string.IsNullOrWhiteSpace(command.Name)) tag.Name = command.Name;
        if (command.Color != null) tag.Color = command.Color;
        if (command.Description != null) tag.Description = command.Description;

        await db.SaveChangesAsync(ct);

        return Result.Ok(new TagItemDto(
            tag.Id,
            tag.ProjectId,
            tag.Path,
            tag.Name,
            tag.Color,
            tag.Description,
            tag.ParentId,
            tag.CreatedAt
        ));
    }
}