namespace Automation.Files.Contracts;

public record ConfirmAssetDto(
    Guid Id,
    string ContentType,
    long Size,
    string PublicUrl
);