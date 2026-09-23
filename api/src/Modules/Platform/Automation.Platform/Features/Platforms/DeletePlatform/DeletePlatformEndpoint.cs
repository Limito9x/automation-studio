namespace Automation.Platform.Features.Platforms.DeletePlatform;

public class DeletePlatformEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/{id:guid}");
        Group<PlatformsGroup>();
        Permissions(P.Platform.Delete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result>(new DeletePlatformCommand(id), ct);
        await this.SendResultAsync(result, ct);
    }
}

