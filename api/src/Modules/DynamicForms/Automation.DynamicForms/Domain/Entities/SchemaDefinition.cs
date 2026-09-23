using Automation.SharedKernel;

namespace Automation.DynamicForms.Domain.Entities;

public class SchemaDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerType { get; set; } = string.Empty;

    // Navigation property
    public ICollection<SchemaVersion> Versions { get; set; } = new List<SchemaVersion>();

    public SchemaDefinition() { }

    public SchemaDefinition(string name, string ownerId, string ownerType)
    {
        Name = name;
        OwnerId = ownerId;
        OwnerType = ownerType;
    }
}

