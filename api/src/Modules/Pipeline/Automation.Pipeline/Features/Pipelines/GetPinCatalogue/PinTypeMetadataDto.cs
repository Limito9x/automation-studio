namespace Automation.Pipeline.Features.Pipelines.GetPinCatalogue;

public record PinTypeMetadataDto(
    string Code,
    string Name,
    string Category,
    string Color,
    string DefaultControl,
    IReadOnlyList<string>? SupportedEntityTargets = null
);
