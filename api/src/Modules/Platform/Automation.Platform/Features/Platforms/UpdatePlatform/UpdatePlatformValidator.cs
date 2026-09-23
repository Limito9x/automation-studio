namespace Automation.Platform.Features.Platforms.UpdatePlatform;

public class UpdatePlatformValidator : Validator<UpdatePlatformCommand>
{
    public UpdatePlatformValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

