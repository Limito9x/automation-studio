using Automation.Identity.Domain;
using Automation.Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Automation.Identity.Features.Users.DeleteUser;

public class DeleteUserHandler(UserManager<User> userManager)
{
    public async Task<Result<string>> HandleAsync(DeleteUserCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString());
        if (user == null || user.IsDeleted)
            return Result.Fail("User not found");

        // Soft delete logic handled by interceptor, so we just Update or Remove.
        // IdentityUser is tracked by UserManager, so we can set IsDeleted manually or just call DeleteAsync.
        // Let's set DeletedAt manually and UpdateAsync so EF Core interceptor triggers (or identity updates it).
        
        user.DeletedAt = DateTimeOffset.UtcNow;
        user.Status = UserStatus.Inactive;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail("Failed to delete user: " + errors);
        }

        return Result.Ok("User deleted successfully");
    }
}



