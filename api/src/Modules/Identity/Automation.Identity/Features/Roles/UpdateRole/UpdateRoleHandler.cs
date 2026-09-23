using Automation.Identity.Domain;
using Automation.Identity.Shared.Dtos;
using Microsoft.AspNetCore.Identity;

namespace Automation.Identity.Features.Roles.UpdateRole;

public class UpdateRoleHandler(RoleManager<Role> roleManager)
{
    public async Task<Result<RoleDto>> HandleAsync(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.Id.ToString());
        if (role == null)
            return Result.Fail("Role not found");

        role.Name = request.Name;

        var result = await roleManager.UpdateAsync(role);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail("Failed to update role: " + errors);
        }

        return Result.Ok(new RoleDto(
            role.Id, 
            role.Name ?? string.Empty,
            role.CreatedAt,
            role.CreatedBy,
            role.UpdatedAt,
            role.UpdatedBy
        ));
    }
}



