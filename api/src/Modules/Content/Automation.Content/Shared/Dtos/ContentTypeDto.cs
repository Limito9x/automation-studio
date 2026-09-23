using System.Text.Json;
using Automation.DynamicForms.Contracts;

namespace Automation.Content.Shared.Dtos;

public record ContentTypeDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public JsonDocument? FieldsConfig { get; set; }
    public JsonDocument DisplayConfig { get; set; } = null!;
    public Dictionary<Guid, SchemaVersionDto> Dependencies { get; set; } = new();
}

