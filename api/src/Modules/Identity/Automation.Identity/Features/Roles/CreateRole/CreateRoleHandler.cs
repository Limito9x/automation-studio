using Automation.Identity.Domain;
using Automation.Identity.Shared.Dtos;
using Microsoft.AspNetCore.Identity;

namespace Automation.Identity.Features.Roles.CreateRole;

public class CreateRoleHandler(RoleManager<Role> roleManager)
{
    public async Task<Result<RoleDto>> HandleAsync(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = new Role
        {
            Name = request.Name
        };

        var result = await roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail("Failed to create role: " + errors);
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



