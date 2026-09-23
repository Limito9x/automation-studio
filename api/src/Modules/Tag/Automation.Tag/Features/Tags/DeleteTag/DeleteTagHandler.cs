using Automation.Tag.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Tag.Features.Tags.DeleteTag;

[Transactional(typeof(TagDbContext))]
public class DeleteTagHandler(TagDbContext db)
{
    public async Task<Result> HandleAsync(DeleteTagCommand command, CancellationToken ct)
    {
        var tag = await db.TagItems.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (tag is null)
            return Result.Ok();

        if (command.DeleteChildren)
        {
            var descendants = await db.TagItems
                .Where(x => x.ProjectId == tag.ProjectId && x.Path.IsDescendantOf(tag.Path))
                .ToListAsync(ct);
            db.TagItems.RemoveRange(descendants);
        }
        else
        {
            db.TagItems.Remove(tag);
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}