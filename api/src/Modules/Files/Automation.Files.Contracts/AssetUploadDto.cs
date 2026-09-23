namespace Automation.Files.Contracts;

public record AssetUploadDto(
    Guid AssetId,
    string HashSha256,
    bool IsAlreadyExists,
    string? PresignedUrl,
    string? PublicUrl
);



