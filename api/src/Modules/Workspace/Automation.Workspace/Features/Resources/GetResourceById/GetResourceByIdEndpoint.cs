using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Resources.GetResourceById;

public class GetResourceByIdEndpoint(IMessageBus bus) : EndpointWithoutRequest<ResourceItemDto>
{
    public override void Configure()
    {
        Get("/{id:guid}");
        Group<ResourcesGroup>();
        Permissions(P.Resource.GetById);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<ResourceItemDto>>(new GetResourceByIdQuery(id), ct);
        await this.SendResultAsync(result, ct);
    }
}

