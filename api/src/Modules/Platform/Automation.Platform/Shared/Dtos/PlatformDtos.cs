namespace Automation.Platform.Shared.Dtos;

public record PlatformDto(
    Guid Id,
    string Key,
    string Name,
    List<string> Extensions,
    DateTimeOffset CreatedAt,
    Guid? IconAssetId = null,
    string? IconUrl = null
);

public record PlatformExtensionDto(
    Guid Id,
    string Extension,
    DateTimeOffset CreatedAt
);

