using Gridify;

namespace Automation.Content.Features.ContentTypes.GetContentTypes;

public class GetContentTypesQuery : PagedQuery
{
    public Guid ProjectId { get; set; }
}

