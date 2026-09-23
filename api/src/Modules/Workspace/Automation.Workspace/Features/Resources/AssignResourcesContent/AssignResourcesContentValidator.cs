namespace Automation.Workspace.Features.Resources.AssignResourcesContent;

public class AssignResourcesContentValidator : AbstractValidator<AssignResourcesContentCommand>
{
    public AssignResourcesContentValidator()
    {
        RuleFor(x => x.ResourceIds)
            .NotEmpty()
            .WithMessage("ResourceIds cannot be empty.");
    }
}
