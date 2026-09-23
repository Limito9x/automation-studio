using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Projects.GetProjects;

public class GetProjectsEndpoint(IMessageBus bus)
    : Endpoint<GetProjectsQuery, PagedResult<ProjectDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<ProjectsGroup>();
        Permissions(P.Project.GetAll);
        Description(x => x.WithName("GetProjects"));
    }

    public override async Task HandleAsync(
        GetProjectsQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<PagedResult<ProjectDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

