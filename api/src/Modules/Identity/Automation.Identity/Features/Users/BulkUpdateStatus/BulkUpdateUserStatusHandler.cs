using Automation.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Automation.Identity.Infrastructure.Auth;
using Wolverine.Attributes;

namespace Automation.Identity.Features.Users.BulkUpdateStatus;

[Transactional(typeof(IdentityDbContext))]
public class BulkUpdateUserStatusHandler(UserManager<User> userManager, IPermissionService permissionService)
{
    public async Task<Result<string>> HandleAsync(BulkUpdateUserStatusCommand command, CancellationToken ct)
    {
        var users = new List<User>();
        foreach (var userId in command.UserIds)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user != null && !user.IsDeleted)
            {
                user.Status = command.TargetStatus;
                users.Add(user);
            }
        }

        if (users.Count == 0)
        {
            return Result.Fail("No valid users found to update.");
        }

        foreach (var user in users)
        {
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Fail($"Failed to update user {user.Id}: {errors}");
            }
            
            // Invalidate cache
            await permissionService.ClearUserStatusCacheAsync(user.Id, ct);
        }

        return Result.Ok($"Successfully updated status for {users.Count} user(s).");
    }
}



