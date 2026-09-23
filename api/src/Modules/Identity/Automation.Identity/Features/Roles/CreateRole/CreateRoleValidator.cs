namespace Automation.Identity.Features.Roles.CreateRole;

public class CreateRoleValidator : Validator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");
    }
}



