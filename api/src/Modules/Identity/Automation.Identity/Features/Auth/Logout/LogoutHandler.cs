using Automation.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Identity.Features.Auth.Logout;

public class LogoutHandler(IdentityDbContext db)
{
    public async Task<Result<string>> HandleAsync(LogoutCommand command, CancellationToken ct)
    {
        var refreshToken = await db
            .RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == command.RefreshToken, ct);

        if (refreshToken is not null && !refreshToken.IsRevoked)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokeReason = "Logout";
            await db.SaveChangesAsync(ct);
        }

        return Result.Ok("Logged out successfully");
    }
}



