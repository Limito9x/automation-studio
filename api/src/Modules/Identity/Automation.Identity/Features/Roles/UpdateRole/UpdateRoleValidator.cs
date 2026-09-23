namespace Automation.Identity.Features.Roles.UpdateRole;

public class UpdateRoleValidator : Validator<UpdateRoleCommand>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");
            
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");
    }
}



