using Automation.Projects.Features.Projects;
using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.ProjectExecutorConfigs.UpsertProjectExecutorConfig;

public class UpsertProjectExecutorConfigEndpoint(IMessageBus bus)
    : Endpoint<UpsertProjectExecutorConfigCommand, ProjectExecutorConfigDto>
{
    public override void Configure()
    {
        Post("{ProjectId:guid}/executor-configs");
        Group<ProjectsGroup>();
        Permissions(P.Project.Update);
        Description(x => x.WithName("UpsertProjectExecutorConfig"));
    }

    public override async Task HandleAsync(
        UpsertProjectExecutorConfigCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ProjectExecutorConfigDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
