using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Projects.DeleteProject;

public class DeleteProjectEndpoint(IMessageBus bus)
    : Endpoint<DeleteProjectCommand>
{
    public override void Configure()
    {
        Delete("/{id}");
        Group<ProjectsGroup>();
        Permissions(P.Project.Delete);
        Description(x => x.WithName("DeleteProject"));
    }

    public override async Task HandleAsync(
        DeleteProjectCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

