using Automation.Platform.Shared.Dtos;

namespace Automation.Platform.Features.PlatformExtensions.CreateExtension;

public class CreateExtensionEndpoint(IMessageBus bus) : Endpoint<CreateExtensionCommand, PlatformExtensionDto>
{
    public override void Configure()
    {
        Post("/");
        Group<PlatformExtensionsGroup>();
        Permissions(P.PlatformExtension.Create);
    }

    public override async Task HandleAsync(CreateExtensionCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<PlatformExtensionDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

