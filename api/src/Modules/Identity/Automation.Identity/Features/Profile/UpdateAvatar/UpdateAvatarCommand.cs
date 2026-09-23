namespace Automation.Identity.Features.Profile.UpdateAvatar;

public record UpdateAvatarCommand(Guid AssetId, string FileName)
{
    public Guid UserId { get; set; }
}



