namespace Automation.Workspace.Features.Resources.DeleteResource;

public class DeleteResourceEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/{id:guid}");
        Group<ResourcesGroup>();
        Permissions(P.Resource.Delete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result>(new DeleteResourceCommand(id), ct);
        await this.SendResultAsync(result, ct);
    }
}

