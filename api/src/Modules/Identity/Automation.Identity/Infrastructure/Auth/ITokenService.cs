using System.Security.Claims;
using Automation.Identity.Domain;

namespace Automation.Identity.Infrastructure.Auth;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}



