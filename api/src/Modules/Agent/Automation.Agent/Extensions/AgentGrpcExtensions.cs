using Automation.Agent.Features.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Agent.Extensions;

public static class AgentGrpcExtensions
{
    public static IServiceCollection AddAgentGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();
        services.AddSingleton<IAgentConnectionRegistry, AgentConnectionRegistry>();
        services.AddSingleton<ICommandTracker, CommandTracker>();
        return services;
    }

    public static IEndpointRouteBuilder MapAgentGrpcServices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<AgentGrpcService>();
        return endpoints;
    }
}
