using Automation.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Automation.Identity.Features.Roles.GetRolePermissions;

public class GetRolePermissionsHandler(RoleManager<Role> roleManager)
{
    public async Task<Result<List<string>>> Handle(GetRolePermissionsQuery query, CancellationToken ct)
    {
        var role = await roleManager.FindByIdAsync(query.Id.ToString());
        if (role is null)
        {
            return Result.Fail($"Role with ID {query.Id} not found");
        }

        var claims = await roleManager.GetClaimsAsync(role);
        var permissions = claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();

        return Result.Ok(permissions);
    }
}




