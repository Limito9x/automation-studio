namespace Automation.Files.Infrastructure.Storage;

public class R2Options
{
    public const string SectionName = "CloudflareR2";

    public string ServiceUrl { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
    public string PublicBaseUrl { get; init; } = string.Empty;
}



