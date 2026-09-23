using Automation.Identity.Domain;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace Automation.Identity.Features.Users.GetUsers;

public class GetUsersHandler(IdentityDbContext db)
{
    public async Task<Result<PagedResult<UserDto>>> HandleAsync(GetUsersQuery query, CancellationToken ct)
    {
        var mapper = new GridifyMapper<User>()
            .GenerateMappings()
            .RemoveMap(nameof(User.PasswordHash))
            .RemoveMap(nameof(User.SecurityStamp));

        var pagedResult = await db.Users
            .AsNoTracking()
            .ToPagedResultAsync(query, mapper, ct);

        if (pagedResult.IsFailed)
            return Result.Fail(pagedResult.Errors);

        var pagedUsers = pagedResult.Value;

        var userIds = pagedUsers.Items.Select(u => u.Id).ToList();

        var userRoles = await db.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name, RoleId = r.Id })
            .ToListAsync(ct);

        var dtos = pagedUsers.Items.Select(u => new UserDto(
            u.Id,
            u.UserName ?? string.Empty,
            u.Email ?? string.Empty,
            u.FirstName,
            u.LastName,
            u.DisplayName,
            u.Status,
            u.PhoneNumber ?? string.Empty,
            u.CreatedAt,
            userRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.RoleName!),
            userRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.RoleId)
        )).ToList();

        return Result.Ok(PagedResult<UserDto>.From(
            dtos, pagedUsers.TotalCount, pagedUsers.Page, pagedUsers.PageSize));
    }
}



