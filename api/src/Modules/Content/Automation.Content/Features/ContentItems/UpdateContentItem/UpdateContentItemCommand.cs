using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automation.Content.Features.ContentItems.UpdateContentItem;

public record UpdateContentItemCommand
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
    public JsonDocument Values { get; set; } = null!;
    public Guid? ThumbnailAssetId { get; set; }
    public string? ThumbnailFileName { get; set; }
}

