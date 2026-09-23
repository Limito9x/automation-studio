using Automation.Files.Contracts;
using Automation.Workspace.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Workspace.Extensions;

public static class WorkspaceAssetExtensions
{
    public static IServiceCollection AddResourceAssetSlots(this IServiceCollection services)
    {
        services.AddAssetSlot(
            entityType: "ResourceVersion",
            slotKey: ResourceAssetSlots.ResourceVersion,
            options: new AssetCategoryOptions
            {
                AllowMultiple = false,
                MaxCount = 1,
                MaxSizeBytes = 500L * 1024 * 1024, // 500MB
                AllowedContentTypes = null // Allow any content type, extension is validated by Platform
            }
        );

        return services;
    }
}

