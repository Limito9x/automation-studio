using Automation.Tag.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Tag.Features.TagLinks.DeleteTagLink;

[Transactional(typeof(TagDbContext))]
public class DeleteTagLinkHandler(TagDbContext db)
{
    public async Task<Result> HandleAsync(DeleteTagLinkCommand command, CancellationToken ct)
    {
        var link = await db.TagLinks.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (link is null)
            return Result.Ok();

        db.TagLinks.Remove(link);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}