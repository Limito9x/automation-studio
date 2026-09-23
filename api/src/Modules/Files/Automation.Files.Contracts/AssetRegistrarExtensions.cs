using Microsoft.Extensions.DependencyInjection;

namespace Automation.Files.Contracts;

public class RegisteredAssetSlot
{
    public string EntityType { get; }
    public string SlotKey { get; }
    public AssetCategoryOptions Options { get; }

    public RegisteredAssetSlot(string entityType, string slotKey, AssetCategoryOptions options)
    {
        EntityType = entityType;
        SlotKey = slotKey;
        Options = options;
    }
}

public static class AssetRegistrarExtensions
{
    public static IServiceCollection AddAssetSlot(
        this IServiceCollection services, 
        string entityType, 
        string slotKey, 
        AssetCategoryOptions options)
    {
        services.AddSingleton(new RegisteredAssetSlot(entityType, slotKey, options));
        return services;
    }
}



