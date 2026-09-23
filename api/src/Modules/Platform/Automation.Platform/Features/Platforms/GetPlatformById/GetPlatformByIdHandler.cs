using Automation.Files.Contracts;
using Automation.Platform.Constants;
using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

using Wolverine.Attributes;

namespace Automation.Platform.Features.Platforms.GetPlatformById;

[NonTransactional]
public class GetPlatformByIdHandler(PlatformDbContext db, IAssetApi assetApi)
{
    public async Task<Result<PlatformDto>> HandleAsync(GetPlatformByIdQuery query, CancellationToken ct)
    {
        var platform = await db.Platforms
            .AsNoTracking()
            .Include(x => x.Extensions)
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct);

        if (platform is null)
            return Result.Fail($"Platform with ID '{query.Id}' was not found.");

        Guid? iconAssetId = null;
        string? iconUrl = null;
        var filesRes = await assetApi.GetFilesAsync(platform.Id.ToString(), "Platform", PlatformAssetSlots.Icon, ct);
        if (filesRes.IsSuccess)
        {
            var firstFile = filesRes.Value.FirstOrDefault();
            if (firstFile != null)
            {
                iconAssetId = firstFile.AssetId;
                iconUrl = firstFile.PublicUrl;
            }
        }

        var dto = new PlatformDto(
            platform.Id,
            platform.Key,
            platform.Name,
            platform.Extensions.Select(e => e.Extension).ToList(),
            platform.CreatedAt,
            iconAssetId,
            iconUrl
        );

        return Result.Ok(dto);
    }
}

