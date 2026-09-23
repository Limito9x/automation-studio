using Automation.DynamicForms.Services;
using Automation.DynamicForms.Services.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.DynamicForms.Extensions;

public static class DynamicFormEngineExtensions
{
    public static IServiceCollection AddDynamicFormEngine(this IServiceCollection services)
    {
        services.AddScoped<IFieldTypeProcessor, DefaultFieldProcessor>();
        services.AddScoped<IFieldTypeProcessor, FileFieldProcessor>();
        services.AddScoped<IFieldTypeProcessor, StructFieldProcessor>();
        services.AddScoped<IDynamicFormEngine, DynamicFormEngine>();
        return services;
    }
}
