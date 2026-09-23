using Automation.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Automation.Identity.Constants;

namespace Automation.Identity.Features.Profile.UpdateProfile;

public class UpdateProfileHandler(
    UserManager<User> userManager,
    ICacheService cacheService)
{
    public async Task<Result<string>> HandleAsync(UpdateProfileCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
            return Result.Fail("User not found");

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.DisplayName = command.DisplayName;
        user.PhoneNumber = command.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail("Failed to update profile: " + errors);
        }

        // Invalidate cache
        await cacheService.RemoveAsync(IdentityCacheKeys.Profile(command.UserId), ct);

        return Result.Ok("Profile updated successfully");
    }
}



