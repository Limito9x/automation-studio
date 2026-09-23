using Automation.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Automation.Identity.Features.Users.UpdateUser;

public class UpdateUserHandler(UserManager<User> userManager)
{
    public async Task<Result<string>> HandleAsync(UpdateUserCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString());
        if (user == null || user.IsDeleted)
            return Result.Fail("User not found");

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.DisplayName = string.IsNullOrWhiteSpace(command.DisplayName) 
            ? $"{command.FirstName} {command.LastName}".Trim() 
            : command.DisplayName;
        user.PhoneNumber = command.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail("Failed to update user: " + errors);
        }

        return Result.Ok("User updated successfully");
    }
}



