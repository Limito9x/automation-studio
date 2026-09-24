using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Studios.AttachRunnerToStudio;

public class AttachRunnerToStudioEndpoint(IMessageBus bus)
    : Endpoint<AttachRunnerToStudioCommand, StudioRunnerDto>
{
    public override void Configure()
    {
        Post("/{studioId:guid}/runners");
        Group<StudiosGroup>();
        Permissions(P.Studio.Update);
        Description(x => x.WithName("AttachRunnerToStudio"));
    }

    public override async Task HandleAsync(AttachRunnerToStudioCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<StudioRunnerDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
