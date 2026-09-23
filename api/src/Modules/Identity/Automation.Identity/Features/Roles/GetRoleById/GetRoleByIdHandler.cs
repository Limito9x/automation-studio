using Automation.Identity.Domain;
using Automation.Identity.Shared.Dtos;
using Microsoft.AspNetCore.Identity;

namespace Automation.Identity.Features.Roles.GetRoleById;

public class GetRoleByIdHandler(RoleManager<Role> roleManager)
{
    public async Task<Result<RoleDto>> HandleAsync(
        GetRoleByIdQuery query,
        CancellationToken ct)
    {
        var role = await roleManager.FindByIdAsync(query.Id.ToString());
        if (role == null)
            return Result.Fail("Role not found");

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



