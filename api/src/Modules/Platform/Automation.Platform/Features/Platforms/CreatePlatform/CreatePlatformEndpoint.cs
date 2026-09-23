using Automation.Platform.Shared.Dtos;

namespace Automation.Platform.Features.Platforms.CreatePlatform;

public class CreatePlatformEndpoint(IMessageBus bus) : Endpoint<CreatePlatformCommand, PlatformDto>
{
    public override void Configure()
    {
        Post("/");
        Group<PlatformsGroup>();
        Permissions(P.Platform.Create);
    }

    public override async Task HandleAsync(CreatePlatformCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<PlatformDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

