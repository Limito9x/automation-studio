using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.ResourceVersions.GetResourceVersions;

public class GetResourceVersionsEndpoint(IMessageBus bus) : EndpointWithoutRequest<IReadOnlyList<ResourceVersionDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<ResourceVersionsGroup>();
        Permissions(P.Resource.GetAll);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var resourceId = Query<Guid>("resourceId");
        var result = await bus.InvokeAsync<Result<IReadOnlyList<ResourceVersionDto>>>(new GetResourceVersionsQuery(resourceId), ct);
        await this.SendResultAsync(result, ct);
    }
}

