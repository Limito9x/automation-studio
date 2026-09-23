namespace Automation.Platform.Features.PlatformExtensions.CreateExtension;

public class CreateExtensionValidator : Validator<CreateExtensionCommand>
{
    public CreateExtensionValidator()
    {
        RuleFor(x => x.Extension)
            .NotEmpty()
            .MaximumLength(50);
    }
}

