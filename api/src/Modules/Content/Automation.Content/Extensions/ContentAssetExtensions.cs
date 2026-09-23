using Automation.Content.Domain.Entities;
using Automation.Files.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Automation.Content.Constants;

namespace Automation.Content.Extensions;

public static class ContentAssetExtensions
{
    public static IServiceCollection AddContentAsset(this IServiceCollection services)
    {
        services.AddAssetSlot(
            nameof(ContentItem),
            ContentAssetSlots.ContentThumbnail,
            new AssetCategoryOptions
            {
                AllowMultiple = false,
                MaxCount = 1,
                MaxSizeBytes = 10 * 1024 * 1024,
                AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"]
            }
        );

        return services;
    }
}