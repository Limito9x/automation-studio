namespace Automation.Files.Contracts;

public record UploadRequestItemDto(string HashSha256, string Extension, long SizeBytes, string ContentType);



