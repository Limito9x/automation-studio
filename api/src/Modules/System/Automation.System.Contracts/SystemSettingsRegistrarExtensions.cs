using Microsoft.Extensions.DependencyInjection;

namespace Automation.SystemAbstractions;

public static class SystemSettingsRegistrarExtensions
{
    public static IServiceCollection AddSystemSetting(
        this IServiceCollection services, 
        string key, 
        string defaultValue, 
        string valueType, 
        string? description = null)
    {
        services.AddSingleton(new RegisteredSystemSetting(key, defaultValue, valueType, description));
        return services;
    }
}



