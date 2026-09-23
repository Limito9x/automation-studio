using System.Text.Json;
using Automation.SharedKernel;

namespace Automation.DynamicForms.Domain.Entities;

public class SchemaData : BaseEntity
{
    public Guid SchemaVersionId { get; set; }
    public JsonDocument Values { get; set; } = null!;
    public string ClientId { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;

    // Navigation property
    public SchemaVersion SchemaVersion { get; set; } = null!;

    public SchemaData() { }

    public SchemaData(Guid schemaVersionId, JsonDocument values, string clientId, string clientType)
    {
        SchemaVersionId = schemaVersionId;
        Values = values;
        ClientId = clientId;
        ClientType = clientType;
    }

    public void UpdateValues(JsonDocument newValues, Guid newSchemaVersionId)
    {
        Values = newValues;
        SchemaVersionId = newSchemaVersionId;
    }
}

