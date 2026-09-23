using Automation.Agent.Infrastructure.Persistence;

namespace Automation.Agent.Features.Agents.RevokeAgent;

public class RevokeAgentHandler(AgentDbContext db)
{
    public async Task<Result> HandleAsync(RevokeAgentCommand command, CancellationToken ct)
    {
        var agent = await db.Agents.FindAsync([command.Id], ct);
        if (agent is null)
            return Result.Fail($"Agent with ID '{command.Id}' was not found.");

        agent.SetActiveStatus(false);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

