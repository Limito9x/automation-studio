using Automation.Files.Contracts;
using Automation.Platform.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Platform.Extensions;

public static class PlatformAssetExtensions
{
    public static IServiceCollection AddPlatformAssetSlots(this IServiceCollection services)
    {
        services.AddAssetSlot(
            entityType: "Platform",
            slotKey: PlatformAssetSlots.Icon,
            options: new AssetCategoryOptions
            {
                AllowMultiple = false,
                MaxCount = 1,
                MaxSizeBytes = 5 * 1024 * 1024,
                AllowedContentTypes = ["image/jpeg", "image/png", "image/webp", "image/svg+xml"]
            }
        );
        return services;
    }
}

