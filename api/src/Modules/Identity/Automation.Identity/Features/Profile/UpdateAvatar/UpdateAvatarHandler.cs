using Automation.Files.Contracts;
using Automation.Identity.Constants;

using Automation.Identity.Infrastructure.Persistence;
using Wolverine.Attributes;

namespace Automation.Identity.Features.Profile.UpdateAvatar;

[Transactional(typeof(IdentityDbContext))]
public class UpdateAvatarHandler(
    IAssetApi assetApi,
    ICacheService cacheService)
{
    public async Task<Result<string>> HandleAsync(
        UpdateAvatarCommand request,
        CancellationToken cancellationToken)
    {
        var linkResult = await assetApi.VerifyAndLinkAsync(
            request.AssetId, 
            "User", 
            "Avatar", 
            request.UserId.ToString(), 
            request.FileName, 
            0, 
            cancellationToken);

        if (linkResult.IsFailed) return linkResult.ToResult<string>();

        // Invalidate cache
        await cacheService.RemoveAsync(IdentityCacheKeys.Profile(request.UserId), cancellationToken);

        return Result.Ok("Avatar updated successfully.");
    }
}



