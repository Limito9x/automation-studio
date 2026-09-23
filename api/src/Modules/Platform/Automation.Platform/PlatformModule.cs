using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Automation.Platform.Infrastructure.Persistence;

using Automation.Platform.Extensions;

namespace Automation.Platform;

public sealed class PlatformModule : IModule, IPermissionModule
{
    public string Name => "Platform";
    public string SchemaName => "platform";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<PlatformDbContext>(config, SchemaName);
        services.AddScoped<Automation.Platform.Contracts.IPlatformApi, Automation.Platform.Infrastructure.PlatformApiService>();
        services.AddPlatformAssetSlots();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.PlatformPermissions().GetPermissions();

    public List<Type> Endpoints => [..DiscoveredTypes.All];
}

