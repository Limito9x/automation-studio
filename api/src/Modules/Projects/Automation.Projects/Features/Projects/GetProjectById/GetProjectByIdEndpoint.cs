using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Projects.GetProjectById;

public class GetProjectByIdEndpoint(IMessageBus bus)
    : Endpoint<GetProjectByIdQuery, ProjectDto>
{
    public override void Configure()
    {
        Get("/{id}");
        Group<ProjectsGroup>();
        Permissions(P.Project.GetById);
        Description(x => x.WithName("GetProjectById"));
    }

    public override async Task HandleAsync(
        GetProjectByIdQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ProjectDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

