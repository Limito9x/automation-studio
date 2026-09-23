namespace Automation.Identity.Features.Profile.UpdateAvatar;

public class UpdateAvatarValidator : AbstractValidator<UpdateAvatarCommand>
{
    public UpdateAvatarValidator()
    {
        RuleFor(x => x.AssetId).NotEmpty().WithMessage("AssetId is required.");
    }
}



