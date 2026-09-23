using FastEndpoints;
using FluentValidation;

namespace Automation.Identity.Features.Roles.UpdateRolePermissions;

public class UpdateRolePermissionsValidator : Validator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");

        RuleFor(x => x.Permissions)
            .NotNull()
            .WithMessage("Permissions list is required");
    }
}



