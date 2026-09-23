namespace Automation.Platform.Features.PlatformExtensions.DeleteExtension;

public class DeleteExtensionEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/{id:guid}");
        Group<PlatformExtensionsGroup>();
        Permissions(P.PlatformExtension.Delete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result>(new DeleteExtensionCommand(id), ct);
        await this.SendResultAsync(result, ct);
    }
}

