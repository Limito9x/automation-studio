using FluentValidation;

namespace Automation.Identity.Features.Auth.ForgotPassword;

public class ForgotPasswordValidator : Validator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}



