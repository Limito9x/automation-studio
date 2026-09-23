using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentTypes.DeleteContentType;

public class DeleteContentTypeHandler(ContentDbContext db)
{
    public async Task<Result> HandleAsync(
        DeleteContentTypeCommand command,
        CancellationToken ct)
    {
        var item = await db.ContentTypes.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (item is null) return Result.Fail(new NotFoundError("ContentType not found"));
        
        db.ContentTypes.Remove(item);
        await db.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}

