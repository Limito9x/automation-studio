using System.Security.Cryptography;
using Automation.SharedKernel.Abstractions.Caching;
using Wolverine.Attributes;

namespace Automation.Agent.Features.Agents.GenerateSetupToken;

[NonTransactional]
public class GenerateSetupTokenHandler(ICacheService cache)
{
    public async Task<Result<SetupTokenDto>> HandleAsync(GenerateSetupTokenCommand command, CancellationToken ct)
    {
        var tokenBytes = new byte[12];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var token = "AGT-" + Convert.ToHexString(tokenBytes);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        var cacheKey = $"agent_setup_token:{token}";

        await cache.SetAsync(cacheKey, true, TimeSpan.FromMinutes(30), ct);

        return Result.Ok(new SetupTokenDto(token, expiresAt));
    }
}
