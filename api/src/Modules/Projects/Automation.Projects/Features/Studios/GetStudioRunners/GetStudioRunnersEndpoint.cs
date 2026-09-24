using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Studios.GetStudioRunners;

public class GetStudioRunnersEndpoint(IMessageBus bus)
    : Endpoint<GetStudioRunnersQuery, IReadOnlyList<StudioRunnerDto>>
{
    public override void Configure()
    {
        Get("/{studioId:guid}/runners");
        Group<StudiosGroup>();
        Permissions(P.Studio.GetById);
        Description(x => x.WithName("GetStudioRunners"));
    }

    public override async Task HandleAsync(GetStudioRunnersQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<StudioRunnerDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
