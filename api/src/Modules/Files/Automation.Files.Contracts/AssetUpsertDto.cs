namespace Automation.Files.Contracts;

public class AssetUpsertDto{
    public Guid AssetId { get; set; }
    public string Name { get; set; } = String.Empty;
}