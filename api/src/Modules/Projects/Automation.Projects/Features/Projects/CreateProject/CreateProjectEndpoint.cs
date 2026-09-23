using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Projects.CreateProject;

public class CreateProjectEndpoint(IMessageBus bus)
    : Endpoint<CreateProjectCommand, ProjectDto>
{
    public override void Configure()
    {
        Post("/"); // Change this method/route accordingly
        Group<ProjectsGroup>();
        Permissions(P.Project.Create);
        Description(x => x.WithName("CreateProject"));
    }

    public override async Task HandleAsync(
        CreateProjectCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ProjectDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

