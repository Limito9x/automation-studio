using FluentValidation;

namespace Automation.Projects.Features.Studios.CreateStudio;

public class CreateStudioValidator : AbstractValidator<CreateStudioCommand>
{
    public CreateStudioValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Studio name is required")
            .MaximumLength(255).WithMessage("Studio name must not exceed 255 characters");

        RuleFor(x => x.Slug)
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Slug));
    }
}
