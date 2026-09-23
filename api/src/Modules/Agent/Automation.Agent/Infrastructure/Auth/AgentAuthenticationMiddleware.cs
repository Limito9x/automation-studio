using Automation.Agent.Infrastructure.Persistence;
using Automation.SharedKernel.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Automation.Agent.Infrastructure.Auth;

public class AgentAuthenticationMiddleware(
    RequestDelegate next,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<AgentAuthenticationMiddleware> logger)
{
    public const string AgentKeyHeaderName = "X-Agent-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(AgentKeyHeaderName, out var agentKeyValues))
        {
            var agentKey = agentKeyValues.ToString();
            if (!string.IsNullOrWhiteSpace(agentKey))
            {
                using var scope = serviceScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();

                var agent = await db.Agents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.RegistrationToken == agentKey && a.IsActive, context.RequestAborted);

                if (agent is not null)
                {
                    context.Items[CurrentAgent.HttpContextItemKey] = agent.Id;

                    // Update LastSeenAt async without blocking request
                    var agentId = agent.Id;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var updateScope = serviceScopeFactory.CreateScope();
                            var updateDb = updateScope.ServiceProvider.GetRequiredService<AgentDbContext>();
                            var updateAgent = await updateDb.Agents.FindAsync(agentId);
                            if (updateAgent is not null)
                            {
                                updateAgent.UpdateLastSeen();
                                await updateDb.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Failed to update LastSeenAt for Agent {AgentId}", agentId);
                        }
                    });
                }
            }
        }

        await next(context);
    }
}

public static class AgentAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseAgentAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AgentAuthenticationMiddleware>();
    }
}

