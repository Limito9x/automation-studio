namespace Automation.Platform.Features.Platforms.UpdatePlatform;

public record UpdatePlatformCommand(Guid Id, string Name, List<string>? Extensions = null, Guid? IconAssetId = null);

public class UpdatePlatformRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string>? Extensions { get; set; }
    public Guid? IconAssetId { get; set; }
}

