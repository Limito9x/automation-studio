using Automation.Identity.Domain;
using Automation.Identity.Infrastructure.Persistence;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace Automation.Identity.Features.Roles.GetRoles;

public class GetRolesHandler(IdentityDbContext db)
{
    public async Task<Result<PagedResult<RoleDto>>> HandleAsync(
        GetRolesQuery query,
        CancellationToken ct)
    {
        var mapper = new GridifyMapper<Role>()
            .GenerateMappings();

        var pagedResult = await db.Roles
            .AsNoTracking()
            .ToPagedResultAsync(query, mapper, ct);
            
        if (pagedResult.IsFailed)
            return Result.Fail(pagedResult.Errors);

        var dtos = pagedResult.Value.Items.Select(r => new RoleDto(
            r.Id, 
            r.Name ?? string.Empty, 
            r.CreatedAt, 
            r.CreatedBy, 
            r.UpdatedAt, 
            r.UpdatedBy
        )).ToList();

        return Result.Ok(PagedResult<RoleDto>.From(
            dtos, pagedResult.Value.TotalCount, pagedResult.Value.Page, pagedResult.Value.PageSize));
    }
}



