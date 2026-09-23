using Automation.DynamicForms.Constants;
using Automation.DynamicForms.Domain.Entities;
using Automation.Files.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.DynamicForms.Extensions;

public static class DynamicFormAssetExtensions{
    public static IServiceCollection AddDynamicFormAssets(this IServiceCollection services){
        services.AddAssetSlot(
            nameof(SchemaData),
            DynamicFormAssets.SchemaDataAsset,
            new AssetCategoryOptions(){
                AllowMultiple = true
            }   
        );
        return services;
    }
}