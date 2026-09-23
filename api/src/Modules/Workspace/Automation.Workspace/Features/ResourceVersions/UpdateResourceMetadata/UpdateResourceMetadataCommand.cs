using System.Text.Json;

namespace Automation.Workspace.Features.ResourceVersions.UpdateResourceMetadata;

public record UpdateResourceMetadataCommand(
    Guid ResourceVersionId,
    JsonElement Data
);
