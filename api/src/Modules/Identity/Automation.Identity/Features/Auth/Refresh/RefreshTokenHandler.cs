using Automation.Identity.Domain;
using Automation.Identity.Infrastructure.Auth;
using Automation.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Identity.Features.Auth.Refresh;

public class RefreshTokenHandler(ITokenService tokenService, IdentityDbContext db)
{
    public async Task<Result<RefreshTokenResult>> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken ct
    )
    {
        var refreshToken = await db
            .RefreshTokens.Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == command.Token, ct);

        if (refreshToken is null)
            return Result.Fail("Refresh token not found");

        if (refreshToken.IsRevoked)
        {
            await RevokeAllUserTokensAsync(refreshToken.UserId, "Suspicious reuse detected", ct);
            return Result.Fail("Token has been revoked");
        }

        if (refreshToken.IsExpired)
            return Result.Fail("Token is expired");

        var newRefreshTokenValue = tokenService.GenerateRefreshToken();
        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenValue,
            UserId = refreshToken.UserId,
            IpAddress = command.IpAddress,
            UserAgent = command.UserAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByToken = newRefreshTokenValue;
        refreshToken.RevokeReason = "Replaced";

        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(ct);

        var accessToken = tokenService.GenerateAccessToken(refreshToken.User);

        return Result.Ok(
            new RefreshTokenResult(accessToken, newRefreshTokenValue, newRefreshToken.ExpiresAt)
        );
    }

    private async Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct)
    {
        var activeTokens = await db
            .RefreshTokens.Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokeReason = reason;
        }

        await db.SaveChangesAsync(ct);
    }
}



