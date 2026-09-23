using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automation.Content.Features.ContentItems.CreateContentItem;

public record CreateContentItemCommand{
    public Guid ProjectId { get; set; }

    public string Key { get; set; } = null!;

    public string Name { get; set; } = null!;

    public JsonDocument Values { get; set; } = null!;
    public Guid? ThumbnailAssetId { get; set; }
    public string? ThumbnailFileName { get; set; }
};

