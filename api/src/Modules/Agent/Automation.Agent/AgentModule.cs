using Automation.Agent.Extensions;
using Automation.Agent.Infrastructure.Persistence;
using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Automation.Agent;

public sealed class AgentModule : IModule, IPermissionModule
{
    public string Name => "Agent";
    public string SchemaName => "agent";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<AgentDbContext>(config, SchemaName);
        services.AddScoped<
            Automation.SharedKernel.Abstractions.Auth.ICurrentAgent,
            Automation.SharedKernel.Infrastructure.Auth.CurrentAgent
        >();
        services.AddScoped<
            Automation.Agent.Contracts.IAgentApi,
            Automation.Agent.Infrastructure.Services.AgentApiService
        >();
        services.AddAgentGrpcServices();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() =>
        new Constants.AgentPermissions().GetPermissions();

    public List<Type> Endpoints => [.. DiscoveredTypes.All];
}
