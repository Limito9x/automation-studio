using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Automation.Content.Infrastructure.Persistence;
using Automation.DynamicForms.Contracts;
using Automation.Content.Extensions;

namespace Automation.Content;

public sealed class ContentModule : IModule, IPermissionModule
{
    public string Name => "Content";
    public string SchemaName => "content";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<ContentDbContext>(config, SchemaName);
        services.AddDynamicSchema("ContentType");

        services.AddScoped<Automation.Content.Contracts.IContentApi, Automation.Content.Infrastructure.Services.ContentApiService>();
        services.AddContentAsset();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.ContentPermissions().GetPermissions();

    public List<Type> Endpoints => [..DiscoveredTypes.All];
}

