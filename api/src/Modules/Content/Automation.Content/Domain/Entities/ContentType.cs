using System.Text.Json;
using Automation.SharedKernel.Domain.Interfaces;

namespace Automation.Content.Domain.Entities;

public class ContentType : BaseEntity, IAuditTrackable
{
    public Guid ProjectId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public JsonDocument DisplayConfig { get; set; } = null!;

    public ContentType() { }

    public ContentType(Guid projectId, string key, string name, string displayName, string? description, string? icon, string? color, int sortOrder, JsonDocument displayConfig)
    {
        ProjectId = projectId;
        Key = key;
        Name = name;
        DisplayName = displayName;
        Description = description;
        Icon = icon;
        Color = color;
        SortOrder = sortOrder;
        DisplayConfig = displayConfig;
    }

    public void Update(string name, string displayName, string? description, string? icon, string? color, int sortOrder, JsonDocument displayConfig)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        Icon = icon;
        Color = color;
        SortOrder = sortOrder;
        DisplayConfig = displayConfig;
    }
}

