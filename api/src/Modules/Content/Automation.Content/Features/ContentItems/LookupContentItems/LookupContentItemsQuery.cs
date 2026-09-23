namespace Automation.Content.Features.ContentItems.LookupContentItems;

public record LookupContentItemsQuery(
    Guid ProjectId,
    Guid? ContentTypeId = null,
    string? ContentTypeKey = null,
    string? Keyword = null,
    int Limit = 50
);
