using Automation.Platform.Shared.Dtos;

namespace Automation.Platform.Features.Platforms.UpdatePlatform;

public class UpdatePlatformEndpoint(IMessageBus bus) : Endpoint<UpdatePlatformRequest, PlatformDto>
{
    public override void Configure()
    {
        Put("/{id:guid}");
        Group<PlatformsGroup>();
        Permissions(P.Platform.Update);
    }

    public override async Task HandleAsync(UpdatePlatformRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<PlatformDto>>(new UpdatePlatformCommand(id, req.Name, req.Extensions, req.IconAssetId), ct);
        await this.SendResultAsync(result, ct);
    }
}

