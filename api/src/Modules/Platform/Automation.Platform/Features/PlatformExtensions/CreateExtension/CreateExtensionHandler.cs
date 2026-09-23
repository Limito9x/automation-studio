using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Features.PlatformExtensions.CreateExtension;

public class CreateExtensionHandler(PlatformDbContext db)
{
    public async Task<Result<PlatformExtensionDto>> HandleAsync(CreateExtensionCommand command, CancellationToken ct)
    {
        var extensionClean = command.Extension.Trim().TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extensionClean))
            return Result.Fail("Extension cannot be empty.");

        var exists = await db.PlatformExtensions.AnyAsync(
            x => x.Extension == extensionClean || x.Extension == "." + extensionClean, ct);

        if (exists)
            return Result.Fail($"Extension '{extensionClean}' already exists.");

        var ext = new Domain.Entities.PlatformExtension(extensionClean);
        db.PlatformExtensions.Add(ext);
        await db.SaveChangesAsync(ct);

        return Result.Ok(ext.Adapt<PlatformExtensionDto>());
    }
}
