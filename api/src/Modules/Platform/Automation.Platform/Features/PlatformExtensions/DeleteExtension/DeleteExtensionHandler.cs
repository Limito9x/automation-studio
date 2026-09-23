using Automation.Platform.Infrastructure.Persistence;

namespace Automation.Platform.Features.PlatformExtensions.DeleteExtension;

public class DeleteExtensionHandler(PlatformDbContext db)
{
    public async Task<Result> HandleAsync(DeleteExtensionCommand command, CancellationToken ct)
    {
        var ext = await db.PlatformExtensions.FindAsync([command.Id], ct);
        if (ext is null)
            return Result.Fail($"PlatformExtension with ID '{command.Id}' was not found.");

        db.PlatformExtensions.Remove(ext);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

