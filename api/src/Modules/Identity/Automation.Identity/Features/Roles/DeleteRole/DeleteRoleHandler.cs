using Automation.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Automation.Identity.Features.Roles.DeleteRole;

public class DeleteRoleHandler(RoleManager<Role> roleManager)
{
    public async Task<Result> HandleAsync(
        DeleteRoleCommand command,
        CancellationToken ct)
    {
        var role = await roleManager.FindByIdAsync(command.Id.ToString());
        if (role == null)
            return Result.Fail("Role not found");

        var result = await roleManager.DeleteAsync(role);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail("Failed to delete role: " + errors);
        }
        
        return Result.Ok();
    }
}



