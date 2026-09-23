using Automation.Files.Contracts;
using Automation.Identity.Constants;
using Automation.Identity.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Identity.Extensions;

public static class IdentityAssetExtensions
{
    public static IServiceCollection AddIdentityAssetSlots(this IServiceCollection services)
    {
        services.AddAssetSlot(
            nameof(User),
            IdentityAssetSlots.Avatar,
            new AssetCategoryOptions
            {
                AllowMultiple = false,
                MaxCount = 1,
                MaxSizeBytes = 5 * 1024 * 1024, // 5MB
                AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"]
            }
        );

        return services;
    }
}



