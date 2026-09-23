using Automation.Agent.Infrastructure.Persistence;
using Automation.Agent.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Agent.Features.Agents.GetMyAgent;

public class GetMyAgentHandler(AgentDbContext db)
{
    public async Task<Result<AgentDto>> HandleAsync(GetMyAgentQuery query, CancellationToken ct)
    {
        var agent = await db.Agents
            .AsNoTracking()
            .Where(x => x.Id == query.AgentId)
            .ProjectToType<AgentDto>()
            .FirstOrDefaultAsync(ct);

        if (agent is null)
            return Result.Fail("Agent not found or inactive.");

        return Result.Ok(agent);
    }
}

