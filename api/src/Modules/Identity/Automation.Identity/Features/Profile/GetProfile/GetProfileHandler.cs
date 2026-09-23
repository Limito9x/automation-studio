using Automation.Identity.Domain;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Automation.Identity.Constants;
using Wolverine.Attributes;

namespace Automation.Identity.Features.Profile.GetProfile;

[NonTransactional]
public class GetProfileHandler(
    UserManager<User> userManager, 
    Files.Contracts.IAssetApi assetApi,
    ICacheService cacheService)
{
    public async Task<Result<GetProfileResult>> HandleAsync(GetProfileQuery query, CancellationToken ct)
    {
        var cacheKey = IdentityCacheKeys.Profile(query.UserId);
        
        // 1. Try get from cache
        var cachedProfile = await cacheService.GetAsync<GetProfileResult>(cacheKey, ct);
        if (cachedProfile != null)
        {
            return Result.Ok(cachedProfile);
        }

        // 2. Fetch from DB
        var user = await userManager.FindByIdAsync(query.UserId.ToString());
        if (user == null)
            return Result.Fail("User not found");

        string? avatarUrl = null;
        var avatarResult = await assetApi.GetFilesAsync(user.Id.ToString(), nameof(User), IdentityAssetSlots.Avatar, ct);
        if (avatarResult.IsSuccess)
        {
            avatarUrl = avatarResult.Value.FirstOrDefault()?.PublicUrl;
        }

        var result = user.Adapt<GetProfileResult>() with { AvatarUrl = avatarUrl };

        // 3. Set cache
        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15), ct);

        return Result.Ok(result);
    }
}



