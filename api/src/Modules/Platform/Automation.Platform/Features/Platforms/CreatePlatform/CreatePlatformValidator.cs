namespace Automation.Platform.Features.Platforms.CreatePlatform;

public class CreatePlatformValidator : Validator<CreatePlatformCommand>
{
    public CreatePlatformValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Platform Key is required.")
            .MaximumLength(100);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Platform Name is required.")
            .MaximumLength(100);
    }
}

