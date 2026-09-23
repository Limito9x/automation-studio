using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automation.Content.Features.ContentTypes.CreateContentType;

public record CreateContentTypeCommand{
    public Guid ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public int SortOrder { get; set; }

    public JsonDocument? DisplayConfig { get; set; }
}

