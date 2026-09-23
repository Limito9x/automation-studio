namespace Automation.Files.Contracts;

public class AssetCategoryOptions
{
    public bool AllowMultiple { get; init; } = false;
    public int? MaxCount { get; init; }
    public long MaxSizeBytes { get; init; } = 5 * 1024 * 1024; // 5MB default
    public string[]? AllowedContentTypes { get; init; }
}



