using Automation.Identity.Domain;
using Automation.Identity.Domain.Enums;
using Automation.Identity.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;

namespace Automation.Identity.Features.Auth.Login;

public class LoginHandler(
    ITokenService tokenService,
    UserManager<User> userManager,
    IdentityDbContext db
)
{
    public async Task<Result<LoginResult>> HandleAsync(LoginCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, command.Password))
        {
            return Result.Fail("Invalid email or password");
        }

        if(user.Status != UserStatus.Active)
        {
            return Result.Fail("User is not active");
        }

        if (user.MustChangePassword)
        {
            return Result.Fail("You must reset your password before logging in.");
        }

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshTokenValue = tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IpAddress = command.IpAddress,
            UserAgent = command.UserAgent,
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(ct);

        var result = new LoginResult(accessToken, refreshTokenValue, refreshToken.ExpiresAt);
        return Result.Ok(result);
    }
}



