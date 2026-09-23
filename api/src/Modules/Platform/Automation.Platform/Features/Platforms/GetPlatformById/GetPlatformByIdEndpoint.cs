using Automation.Platform.Shared.Dtos;

namespace Automation.Platform.Features.Platforms.GetPlatformById;

public class GetPlatformByIdEndpoint(IMessageBus bus) : EndpointWithoutRequest<PlatformDto>
{
    public override void Configure()
    {
        Get("/{id:guid}");
        Group<PlatformsGroup>();
        Permissions(P.Platform.GetById);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<PlatformDto>>(new GetPlatformByIdQuery(id), ct);
        await this.SendResultAsync(result, ct);
    }
}

