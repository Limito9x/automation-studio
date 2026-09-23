namespace Automation.Identity.Features.Profile.ChangePassword;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword)
{
    public Guid UserId { get; set; }
}



