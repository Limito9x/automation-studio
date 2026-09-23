using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Automation.Identity.Infrastructure.Auth;

public class PermissionClaimsTransformation(IPermissionService permissionService)
    : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            return principal;

        // Check if user is active
        var isActive = await permissionService.IsUserActiveAsync(userId);
        if (!isActive)
        {
            // Return an unauthenticated principal which will result in 401 Unauthorized
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var permissions = await permissionService.GetPermissionsAsync(userId);

        var identity = (ClaimsIdentity)principal.Identity;

        if (!identity.HasClaim(c => c.Type == "Permission"))
        {
            identity.AddClaims(permissions.Select(p => new Claim("Permission", p)));
        }

        return principal;
    }
}



