namespace Automation.DynamicForms.Features.Structs.UpdateStruct;

public class UpdateStructValidator : AbstractValidator<UpdateStructCommand>
{
    public UpdateStructValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9_\s-]+$")
            .WithMessage("Name can only contain alphanumeric characters, spaces, underscores, and hyphens.");
    }
}
