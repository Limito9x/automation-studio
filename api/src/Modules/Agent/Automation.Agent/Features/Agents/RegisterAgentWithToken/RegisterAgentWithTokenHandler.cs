using System.Security.Cryptography;
using Automation.Agent.Infrastructure.Persistence;
using Automation.Agent.Shared.Dtos;
using Automation.SharedKernel.Abstractions.Caching;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Agent.Features.Agents.RegisterAgentWithToken;

[Transactional(typeof(AgentDbContext))]
public class RegisterAgentWithTokenHandler(AgentDbContext db, ICacheService cache)
{
    public async Task<Result<RegisterAgentResultDto>> HandleAsync(RegisterAgentWithTokenCommand command, CancellationToken ct)
    {
        var cacheKey = $"agent_setup_token:{command.SetupToken}";
        var isValidToken = await cache.GetAsync<bool>(cacheKey, ct);

        if (!isValidToken)
        {
            return Result.Fail("Invalid or expired setup token");
        }

        // Consume the token (single use)
        await cache.RemoveAsync(cacheKey, ct);

        var existingAgent = await db.Agents
            .FirstOrDefaultAsync(x => x.MachineKey == command.MachineKey, ct);

        if (existingAgent is not null)
        {
            if (!existingAgent.IsActive)
            {
                existingAgent.SetActiveStatus(true);
                await db.SaveChangesAsync(ct);
            }

            return Result.Ok(new RegisterAgentResultDto(
                existingAgent.Id,
                existingAgent.Name,
                existingAgent.MachineKey,
                existingAgent.RegistrationToken
            ));
        }

        var tokenBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var registrationToken = Convert.ToBase64String(tokenBytes);

        var agent = new Domain.Entities.Agent(command.Name, command.MachineKey, registrationToken);
        db.Agents.Add(agent);
        await db.SaveChangesAsync(ct);

        return Result.Ok(new RegisterAgentResultDto(
            agent.Id,
            agent.Name,
            agent.MachineKey,
            agent.RegistrationToken
        ));
    }
}
