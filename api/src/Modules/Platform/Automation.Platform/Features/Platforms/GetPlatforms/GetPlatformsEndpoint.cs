using Automation.Platform.Shared.Dtos;

namespace Automation.Platform.Features.Platforms.GetPlatforms;

public class GetPlatformsEndpoint(IMessageBus bus) : EndpointWithoutRequest<IReadOnlyList<PlatformDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<PlatformsGroup>();
        Permissions(P.Platform.GetAll);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<PlatformDto>>>(new GetPlatformsQuery(), ct);
        await this.SendResultAsync(result, ct);
    }
}

