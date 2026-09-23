using FastEndpoints;
using FluentValidation;

namespace Automation.Identity.Features.Profile.ChangePassword;

public class ChangePasswordValidator : Validator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required.");
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).WithMessage("New password must be at least 6 characters long.");
    }
}



