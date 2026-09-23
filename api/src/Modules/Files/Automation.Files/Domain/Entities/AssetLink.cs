namespace Automation.Files.Domain.Entities;

public class AssetLink : AuditableEntity
{
    public Guid AssetId { get; set; }
    public string OwnerEntityType { get; set; } = string.Empty;
    public string SlotKey { get; set; } = string.Empty;
    public string OwnerEntityId { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Asset Asset { get; set; } = null!;

    public AssetLink() { }

    public AssetLink(Guid assetId, string ownerEntityType, string slotKey, string ownerEntityId, string originalName, int sortOrder = 0)
    {
        AssetId = assetId;
        OwnerEntityType = ownerEntityType;
        SlotKey = slotKey;
        OwnerEntityId = ownerEntityId;
        OriginalName = originalName;
        SortOrder = sortOrder;
    }
}



