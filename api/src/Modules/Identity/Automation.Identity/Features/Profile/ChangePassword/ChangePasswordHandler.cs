using Automation.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Wolverine.Attributes;

namespace Automation.Identity.Features.Profile.ChangePassword;

[Transactional(typeof(IdentityDbContext))]
public class ChangePasswordHandler(UserManager<User> userManager)
{
    public async Task<Result<string>> HandleAsync(ChangePasswordCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null || user.IsDeleted)
        {
            return Result.Fail("User not found.");
        }

        var result = await userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail(errors);
        }

        return Result.Ok("Password changed successfully.");
    }
}



