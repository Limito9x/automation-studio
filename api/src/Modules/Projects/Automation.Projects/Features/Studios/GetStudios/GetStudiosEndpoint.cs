using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Studios.GetStudios;

public class GetStudiosEndpoint(IMessageBus bus)
    : Endpoint<GetStudiosQuery, IReadOnlyList<StudioDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<StudiosGroup>();
        Permissions(P.Studio.GetAll);
        Description(x => x.WithName("GetStudios"));
    }

    public override async Task HandleAsync(GetStudiosQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<StudioDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
