using System.Text.Json;

namespace Automation.Tag.Features.TagLinks.CreateTagLink;

public record CreateTagLinkCommand(
    Guid ProjectId,
    string EntityType,
    Guid EntityId,
    string TagPath,
    string? TargetSubPath = null,
    JsonDocument? Metadata = null
);
