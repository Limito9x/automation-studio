using FastEndpoints;
using FluentValidation;

namespace Automation.Identity.Features.Users.BulkUpdateStatus;

public class BulkUpdateUserStatusValidator : Validator<BulkUpdateUserStatusCommand>
{
    public BulkUpdateUserStatusValidator()
    {
        RuleFor(x => x.UserIds).NotEmpty().WithMessage("At least one user ID is required.");
        RuleFor(x => x.TargetStatus).IsInEnum().WithMessage("Invalid target status.");
    }
}



