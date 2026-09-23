using System.Text.Json;
using Automation.SharedKernel;

namespace Automation.DynamicForms.Domain.Entities;

public class SchemaVersion : BaseEntity
{
    public Guid SchemaDefinitionId { get; set; }
    public JsonDocument Fields { get; set; } = null!;
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> DependencySchemaIds { get; set; } = [];

    // Navigation property
    public SchemaDefinition SchemaDefinition { get; set; } = null!;

    public SchemaVersion() { }

    public SchemaVersion(Guid schemaDefinitionId, JsonDocument fields, int version, bool isActive)
    {
        SchemaDefinitionId = schemaDefinitionId;
        Fields = fields;
        Version = version;
        IsActive = isActive;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
    
    public void Activate()
    {
        IsActive = true;
    }
}

