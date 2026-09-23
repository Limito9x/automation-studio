using Automation.Agent.Domain.Entities;
using Automation.Agent.Infrastructure.Persistence;
using Automation.Agent.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Agent.Features.Agents.ReportExecutorConfigs;

[Transactional(typeof(AgentDbContext))]
public class ReportExecutorConfigsHandler(AgentDbContext db)
{
    public async Task<Result<IReadOnlyList<AgentExecutorConfigDto>>> HandleAsync(ReportExecutorConfigsCommand command, CancellationToken ct)
    {
        var agent = await db.Agents
            .Include(x => x.ExecutorConfigs)
            .FirstOrDefaultAsync(x => x.Id == command.AgentId, ct);

        if (agent is null)
            return Result.Fail($"Agent with ID '{command.AgentId}' was not found.");

        foreach (var item in command.Configs)
        {
            var key = item.ExecutorKey.Trim().ToLowerInvariant();
            var existing = agent.ExecutorConfigs
                .FirstOrDefault(x => x.ExecutorKey.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                existing.Update(item.ExecutablePath, item.Version);
            }
            else
            {
                var newConfig = new AgentExecutorConfig(
                    command.AgentId,
                    key,
                    item.ExecutablePath,
                    item.Version
                );
                db.AgentExecutorConfigs.Add(newConfig);
            }
        }

        await db.SaveChangesAsync(ct);

        var configs = await db.AgentExecutorConfigs
            .AsNoTracking()
            .Where(x => x.AgentId == command.AgentId)
            .ProjectToType<AgentExecutorConfigDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<AgentExecutorConfigDto>>(configs);
    }
}
