using Automation.Projects.Features.Projects;

namespace Automation.Projects.Features.ProjectExecutorConfigs.DeleteProjectExecutorConfig;

public class DeleteProjectExecutorConfigEndpoint(IMessageBus bus)
    : Endpoint<DeleteProjectExecutorConfigCommand>
{
    public override void Configure()
    {
        Delete("{ProjectId:guid}/executor-configs/{Id:guid}");
        Group<ProjectsGroup>();
        Permissions(P.Project.Delete);
        Description(x => x.WithName("DeleteProjectExecutorConfig"));
    }

    public override async Task HandleAsync(
        DeleteProjectExecutorConfigCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
