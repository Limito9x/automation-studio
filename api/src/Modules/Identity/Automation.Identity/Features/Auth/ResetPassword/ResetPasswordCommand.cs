namespace Automation.Identity.Features.Auth.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword);



