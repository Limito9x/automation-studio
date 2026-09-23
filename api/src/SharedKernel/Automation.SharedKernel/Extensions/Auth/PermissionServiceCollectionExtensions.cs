using Microsoft.Extensions.DependencyInjection;
using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Abstractions.Auth;


namespace Automation.SharedKernel.Extensions.Auth;

public static class PermissionServiceCollectionExtensions
{
    public static IServiceCollection AddModulePermissions(this IServiceCollection services, IEnumerable<IModule> modules)
    {
        var globalRegistry = new GlobalPermissionRegistry();

        foreach (var module in modules.OfType<IPermissionModule>())
        {
            globalRegistry.Modules.Add(((IModule)module).Name, module.GetPermissions());
        }

        services.AddSingleton(globalRegistry);

        return services;
    }
}



