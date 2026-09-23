using Automation.Files.Contracts;
using Automation.Platform.Constants;
using Automation.Platform.Features.PlatformExtensions.CreateExtensions;
using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Platform.Features.Platforms.UpdatePlatform;

[Transactional(typeof(PlatformDbContext))]
public class UpdatePlatformHandler(PlatformDbContext db, IAssetApi assetApi)
{
    public async Task<Result<PlatformDto>> HandleAsync(UpdatePlatformCommand command, CancellationToken ct)
    {
        var platform = await db.Platforms
            .Include(x => x.Extensions)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (platform is null)
            return Result.Fail($"Platform with ID '{command.Id}' was not found.");

        platform.Update(command.Name);

        if (command.Extensions is not null)
        {
            var extensionEntities = await CreateExtensionsHandler.EnsureExtensionsExistAsync(db, command.Extensions, ct);
            platform.SetExtensions(extensionEntities);
        }

        await db.SaveChangesAsync(ct);

        Guid? currentIconAssetId = command.IconAssetId;
        if (command.IconAssetId.HasValue && command.IconAssetId.Value != Guid.Empty)
        {
            await assetApi.VerifyAndLinkAsync(
                command.IconAssetId.Value,
                "Platform",
                PlatformAssetSlots.Icon,
                platform.Id.ToString(),
                "icon",
                0,
                ct
            );
        }

        string? iconUrl = null;
        var filesRes = await assetApi.GetFilesAsync(platform.Id.ToString(), "Platform", PlatformAssetSlots.Icon, ct);
        if (filesRes.IsSuccess)
        {
            var firstFile = filesRes.Value.FirstOrDefault();
            iconUrl = firstFile?.PublicUrl;
            if (!currentIconAssetId.HasValue && firstFile != null)
            {
                currentIconAssetId = firstFile.AssetId;
            }
        }

        var dto = new PlatformDto(
            platform.Id,
            platform.Key,
            platform.Name,
            platform.Extensions.Select(x => x.Extension).ToList(),
            platform.CreatedAt,
            currentIconAssetId,
            iconUrl
        );

        return Result.Ok(dto);
    }
}

