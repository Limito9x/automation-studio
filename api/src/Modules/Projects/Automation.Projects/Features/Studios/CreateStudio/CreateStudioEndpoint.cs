using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Studios.CreateStudio;

public class CreateStudioEndpoint(IMessageBus bus)
    : Endpoint<CreateStudioCommand, StudioDto>
{
    public override void Configure()
    {
        Post("/");
        Group<StudiosGroup>();
        Permissions(P.Studio.Create);
        Description(x => x.WithName("CreateStudio"));
    }

    public override async Task HandleAsync(CreateStudioCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<StudioDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
