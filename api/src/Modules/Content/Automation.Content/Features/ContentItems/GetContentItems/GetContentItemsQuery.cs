using System.Text.Json.Serialization;

namespace Automation.Content.Features.ContentItems.GetContentItems;

public class GetContentItemsQuery : PagedQuery
{
    public Guid ProjectId { get; set; }

    public string Key { get; set; } = null!;
}

