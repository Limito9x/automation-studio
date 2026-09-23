namespace Automation.Files.Contracts;

public record AssetDto(
    Guid Id,
    string Name,
    string ContentType,
    long Size,
    string PublicUrl
);