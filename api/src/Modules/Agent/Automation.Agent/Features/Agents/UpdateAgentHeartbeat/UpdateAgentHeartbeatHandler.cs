using Automation.Agent.Infrastructure.Persistence;
using Automation.SharedKernel.Abstractions.Agent;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Agent.Features.Agents.UpdateAgentHeartbeat;

[Transactional(typeof(AgentDbContext))]
public class UpdateAgentHeartbeatHandler(AgentDbContext db)
{
    public async Task HandleAsync(UpdateAgentHeartbeatCommand command, CancellationToken ct)
    {
        var agent = await db.Agents.FirstOrDefaultAsync(x => x.Id == command.AgentId, ct);
        if (agent is not null)
        {
            agent.UpdateLastSeen();
            await db.SaveChangesAsync(ct);
        }
    }
}
