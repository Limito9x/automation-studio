using Automation.Projects.Contracts;
using Automation.Projects.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Projects.Extensions;

public static class ProjectsServiceExtensions
{
    public static IServiceCollection AddProjectsServices(this IServiceCollection services)
    {
        services.AddScoped<IProjectsApi, ProjectsApiService>();
        return services;
    }
}
