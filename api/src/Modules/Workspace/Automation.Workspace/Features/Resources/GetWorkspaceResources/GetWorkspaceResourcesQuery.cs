using Automation.SharedKernel.Abstractions.Querying;

namespace Automation.Workspace.Features.Resources.GetWorkspaceResources;

public class GetWorkspaceResourcesQuery : PagedQuery
{
    public Guid WorkspaceId { get; set; }
    public Guid ProjectId { get; set; }
}
