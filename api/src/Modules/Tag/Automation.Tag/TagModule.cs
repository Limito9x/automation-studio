using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Automation.Tag.Contracts;
using Automation.Tag.Infrastructure.Persistence;
using Automation.Tag.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Tag;

public sealed class TagModule : IModule, IPermissionModule
{
    public string Name => "Tag";
    public string SchemaName => "tag";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<TagDbContext>(config, SchemaName);
        services.AddScoped<ITagApi, TagApiService>();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() =>
        new Constants.TagPermissions().GetPermissions();

    public List<Type> Endpoints => [.. DiscoveredTypes.All];
}
