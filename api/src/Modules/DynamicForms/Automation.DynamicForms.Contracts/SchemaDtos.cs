using System.Text.Json;

namespace Automation.DynamicForms.Contracts;

public class SchemaVersionDto
{
    public Guid Id { get; set; }
    public Guid SchemaDefinitionId { get; set; }
    public JsonDocument Fields { get; set; } = null!;
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> DependencySchemaIds { get; set; } = [];
}

public class SchemaDataDto
{
    public Guid Id { get; set; }
    public JsonDocument SchemaVersion { get; set; } = null!;
    public JsonDocument ResolvedData { get; set; } = null!;
    public JsonDocument Values { get; set; } = null!;
    public string ClientId { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;
}

public class StructSummaryDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ActiveVersion { get; set; }
    public int FieldCount { get; set; }
    public List<Guid> DependencySchemaIds { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class StructDetailDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SchemaVersionDto ActiveVersion { get; set; } = null!;
    public Dictionary<Guid, SchemaVersionDto> Dependencies { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class SchemaWithDependenciesDto
{
    public Guid SchemaDefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerType { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public SchemaVersionDto ActiveVersion { get; set; } = null!;
    public Dictionary<Guid, SchemaVersionDto> Dependencies { get; set; } = new();
}

public class SchemaReferencingUsageDto
{
    public Guid SchemaDefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerType { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
}
