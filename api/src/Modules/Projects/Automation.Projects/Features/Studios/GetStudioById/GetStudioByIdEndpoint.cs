using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Studios.GetStudioById;

public class GetStudioByIdEndpoint(IMessageBus bus)
    : Endpoint<GetStudioByIdQuery, StudioDto>
{
    public override void Configure()
    {
        Get("/{id:guid}");
        Group<StudiosGroup>();
        Permissions(P.Studio.GetById);
        Description(x => x.WithName("GetStudioById"));
    }

    public override async Task HandleAsync(GetStudioByIdQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<StudioDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
