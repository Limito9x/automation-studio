using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Extensions;

namespace Automation.Projects;

public sealed class ProjectsModule : IModule, IPermissionModule
{
    public string Name => "Projects";
    public string SchemaName => "projects";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<ProjectsDbContext>(config, SchemaName);
        services.AddProjectsServices();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.ProjectsPermissions().GetPermissions();

    public List<Type> Endpoints => [..DiscoveredTypes.All];
}

