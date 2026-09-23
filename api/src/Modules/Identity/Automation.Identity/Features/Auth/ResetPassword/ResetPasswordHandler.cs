using Automation.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Automation.Identity.Features.Auth.ResetPassword;

public class ResetPasswordHandler(UserManager<User> userManager)
{
    public async Task<Result<string>> HandleAsync(ResetPasswordCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        
        if (user == null)
        {
            return Result.Fail("User not found or invalid token.");
        }

        string decodedToken;
        try
        {
            var decodedBytes = WebEncoders.Base64UrlDecode(command.Token);
            decodedToken = Encoding.UTF8.GetString(decodedBytes);
        }
        catch (FormatException)
        {
            return Result.Fail("Invalid token format.");
        }

        var result = await userManager.ResetPasswordAsync(user, decodedToken, command.NewPassword);

        if (result.Succeeded)
        {
            user.MustChangePassword = false;
            await userManager.UpdateAsync(user);
            return Result.Ok("Password reset successfully.");
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return Result.Fail($"Failed to reset password: {errors}");
    }
}



