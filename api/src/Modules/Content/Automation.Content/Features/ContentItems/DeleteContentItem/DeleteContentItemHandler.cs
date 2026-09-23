using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentItems.DeleteContentItem;

public class DeleteContentItemHandler(ContentDbContext db)
{
    public async Task<Result> HandleAsync(
        DeleteContentItemCommand command,
        CancellationToken ct)
    {
        var item = await db.ContentItems.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (item is null) return Result.Fail(new NotFoundError("ContentItem not found"));
        
        db.ContentItems.Remove(item);
        await db.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}

