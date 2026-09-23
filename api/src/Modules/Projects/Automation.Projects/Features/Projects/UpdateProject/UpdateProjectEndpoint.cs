using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Projects.UpdateProject;

public class UpdateProjectEndpoint(IMessageBus bus)
    : Endpoint<UpdateProjectCommand, ProjectDto>
{
    public override void Configure()
    {
        Put("{Id:guid}");
        Group<ProjectsGroup>();
        Permissions(P.Project.Update);
        Description(x => x.WithName("UpdateProject"));
    }

    public override async Task HandleAsync(
        UpdateProjectCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ProjectDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

