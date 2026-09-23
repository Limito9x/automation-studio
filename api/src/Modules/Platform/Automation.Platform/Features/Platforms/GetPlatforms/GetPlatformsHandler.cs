using Automation.Files.Contracts;
using Automation.Platform.Constants;
using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

using Wolverine.Attributes;

namespace Automation.Platform.Features.Platforms.GetPlatforms;

[NonTransactional]
public class GetPlatformsHandler(PlatformDbContext db, IAssetApi assetApi)
{
    public async Task<Result<IReadOnlyList<PlatformDto>>> HandleAsync(GetPlatformsQuery query, CancellationToken ct)
    {
        var platforms = await db.Platforms
            .AsNoTracking()
            .Include(x => x.Extensions)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        var resultList = new List<PlatformDto>();
        foreach (var platform in platforms)
        {
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

            resultList.Add(new PlatformDto(
                platform.Id,
                platform.Key,
                platform.Name,
                platform.Extensions.Select(e => e.Extension).ToList(),
                platform.CreatedAt,
                iconAssetId,
                iconUrl
            ));
        }

        return Result.Ok<IReadOnlyList<PlatformDto>>(resultList);
    }
}

