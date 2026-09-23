using Automation.Identity.Infrastructure.Persistence;
using Automation.Identity.Shared.Dtos;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Automation.Identity.Features.Roles.GetRoleOptions;

public class GetRoleOptionsHandler(IdentityDbContext db)
{
    public async Task<Result<List<RoleDto>>> HandleAsync(
        GetRoleOptionsQuery query,
        CancellationToken ct)
    {
        var roles = await db.Roles
            .AsNoTracking()
            .Select(r => new RoleDto(
                r.Id, 
                r.Name ?? string.Empty,
                r.CreatedAt,
                r.CreatedBy,
                r.UpdatedAt,
                r.UpdatedBy
            ))
            .ToListAsync(ct);

        return Result.Ok(roles);
    }
}



