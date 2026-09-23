using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Automation.SystemModule.Infrastructure.Persistence;

namespace Automation.SystemModule;

public sealed class SystemModule : IModule, IPermissionModule
{
    public string Name => "System";
    public string SchemaName => "system";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<SystemDbContext>(config, SchemaName);
        services.AddSingleton<SystemAbstractions.ISystemSettingsRegistry, Infrastructure.SystemSettingsRegistry>();
        services.AddHostedService<Infrastructure.SystemSettingsInitializer>();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.SystemPermissions().GetPermissions();

    public List<Type> Endpoints => [.. typeof(SystemModule).Assembly.GetTypes()];
}



