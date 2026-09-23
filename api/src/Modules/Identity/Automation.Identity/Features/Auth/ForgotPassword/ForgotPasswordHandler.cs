using Automation.Identity.Domain;
using Automation.Notifications.Contracts.Messages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Automation.Identity.Features.Auth.ForgotPassword;

public class ForgotPasswordHandler(
    UserManager<User> userManager, 
    IConfiguration configuration,
    IMessageBus bus)
{
    public async Task<Result<string>> HandleAsync(ForgotPasswordCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        
        // Return success even if user not found to prevent enumeration attacks
        if (user == null)
        {
            return Result.Ok("If your email exists in our system, we have sent a password reset link. Please check your inbox.");
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var clientUrl = configuration["AppConfig:ClientUrl"] ?? "http://localhost:5173";
        var resetLink = $"{clientUrl}/auth/reset-password?email={Uri.EscapeDataString(command.Email)}&token={encodedToken}";

        var isRealEmailSend = configuration.GetValue<bool>("AppConfig:Email:RealEmailSend");
        var defaultEmail = configuration["AppConfig:Email:DefaultEmail"] ?? "test@example.com";

        var recipient = isRealEmailSend ? command.Email : defaultEmail;

        var emailCommand = new SendEmailCommand(
            To: recipient,
            Subject: "Reset Password",
            Body: $"<p>You requested a password reset. Please click the link below to reset your password:</p><p><a href=\"{resetLink}\">Reset Password</a></p>",
            IsHtml: true
        );

        await bus.SendAsync(emailCommand);

        return Result.Ok("If your email exists in our system, we have sent a password reset link. Please check your inbox.");
    }
}



