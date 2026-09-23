using Automation.Workspace.Constants;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Resources.GetResourcesByContent;

public class GetResourcesByContentEndpoint(IMessageBus bus)
    : EndpointWithoutRequest<List<ContentResourceDto>>
{
    public override void Configure()
    {
        Get(WorkspaceRoutes.ContentResources);
        Group<ResourcesGroup>();
        Permissions(P.Resource.GetAll);
        Description(x => x.WithName("GetResourcesByContent"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var contentId = Route<Guid>("contentId");
        var result = await bus.InvokeAsync<Result<List<ContentResourceDto>>>(
            new GetResourcesByContentQuery(contentId),
            ct
        );
        await this.SendResultAsync(result, ct);
    }
}
