using Automation.Projects.Features.Projects;
using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.ProjectExecutorConfigs.GetProjectExecutorConfigs;

public class GetProjectExecutorConfigsEndpoint(IMessageBus bus)
    : Endpoint<GetProjectExecutorConfigsQuery, IReadOnlyList<ProjectExecutorConfigDto>>
{
    public override void Configure()
    {
        Get("{ProjectId:guid}/executor-configs");
        Group<ProjectsGroup>();
        Permissions(P.Project.GetAll);
        Description(x => x.WithName("GetProjectExecutorConfigs"));
    }

    public override async Task HandleAsync(
        GetProjectExecutorConfigsQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<ProjectExecutorConfigDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
