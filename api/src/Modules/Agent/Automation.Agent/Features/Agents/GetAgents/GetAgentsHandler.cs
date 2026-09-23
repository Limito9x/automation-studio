using Automation.Agent.Infrastructure.Persistence;
using Automation.Agent.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Agent.Features.Agents.GetAgents;

public class GetAgentsHandler(AgentDbContext db)
{
    public async Task<Result<IReadOnlyList<AgentDto>>> HandleAsync(GetAgentsQuery query, CancellationToken ct)
    {
        IQueryable<Domain.Entities.Agent> dbQuery = db.Agents.AsNoTracking();

        if (query.IsActive.HasValue)
            dbQuery = dbQuery.Where(x => x.IsActive == query.IsActive.Value);

        var agents = await dbQuery
            .Include(x => x.ExecutorConfigs)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectToType<AgentDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<AgentDto>>(agents);
    }
}
