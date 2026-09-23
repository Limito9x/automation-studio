using Automation.Platform.Shared.Dtos;

namespace Automation.Platform.Features.PlatformExtensions.CreateExtensions;

public class CreateExtensionsEndpoint(IMessageBus bus) : Endpoint<CreateExtensionsCommand, IReadOnlyList<PlatformExtensionDto>>
{
    public override void Configure()
    {
        Post("/batch");
        Group<PlatformExtensionsGroup>();
        Permissions(P.PlatformExtension.Create);
    }

    public override async Task HandleAsync(CreateExtensionsCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<PlatformExtensionDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

