namespace Automation.Platform.Features.PlatformExtensions.CreateExtensions;

public class CreateExtensionsValidator : Validator<CreateExtensionsCommand>
{
    public CreateExtensionsValidator()
    {
        RuleFor(x => x.Extensions).NotEmpty();
        RuleForEach(x => x.Extensions)
            .NotEmpty()
            .MaximumLength(50)
            .Must(x => x.StartsWith('.')).WithMessage("Extension must start with a dot (e.g. '.blend', '.fbx').");
    }
}

