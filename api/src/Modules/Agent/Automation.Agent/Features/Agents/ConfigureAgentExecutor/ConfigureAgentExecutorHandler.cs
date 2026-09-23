using Automation.Agent.Domain.Entities;
using Automation.Agent.Infrastructure.Persistence;
using Automation.Agent.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Agent.Features.Agents.ConfigureAgentExecutor;

[Transactional(typeof(AgentDbContext))]
public class ConfigureAgentExecutorHandler(AgentDbContext db)
{
    public async Task<Result<AgentExecutorConfigDto>> HandleAsync(ConfigureAgentExecutorCommand command, CancellationToken ct)
    {
        var agent = await db.Agents
            .Include(x => x.ExecutorConfigs)
            .FirstOrDefaultAsync(x => x.Id == command.AgentId, ct);

        if (agent is null)
            return Result.Fail($"Agent with ID '{command.AgentId}' was not found.");

        var existingConfig = agent.ExecutorConfigs
            .FirstOrDefault(x => x.ExecutorKey.Equals(command.ExecutorKey, StringComparison.OrdinalIgnoreCase));

        if (existingConfig is not null)
        {
            existingConfig.Update(command.ExecutablePath, command.Version);
        }
        else
        {
            existingConfig = new AgentExecutorConfig(
                command.AgentId,
                command.ExecutorKey.ToLowerInvariant(),
                command.ExecutablePath,
                command.Version
            );
            db.AgentExecutorConfigs.Add(existingConfig);
        }

        await db.SaveChangesAsync(ct);

        var dto = existingConfig.Adapt<AgentExecutorConfigDto>();
        return Result.Ok(dto);
    }
}
