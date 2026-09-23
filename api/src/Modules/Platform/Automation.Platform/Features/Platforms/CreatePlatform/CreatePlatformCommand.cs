namespace Automation.Platform.Features.Platforms.CreatePlatform;

public record CreatePlatformCommand(
    string Key,
    string Name,
    List<string>? Extensions = null,
    Guid? IconAssetId = null
);

