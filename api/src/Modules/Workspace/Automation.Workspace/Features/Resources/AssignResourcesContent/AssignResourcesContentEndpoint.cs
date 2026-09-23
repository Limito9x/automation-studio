using Automation.Workspace.Constants;

namespace Automation.Workspace.Features.Resources.AssignResourcesContent;

public class AssignResourcesContentEndpoint(IMessageBus bus)
    : Endpoint<AssignResourcesContentCommand>
{
    public override void Configure()
    {
        Put(WorkspaceRoutes.AssignResourcesContent);
        Group<ResourcesGroup>();
        Permissions(P.Resource.Update);
        Description(x => x.WithName("AssignResourcesContent"));
    }

    public override async Task HandleAsync(AssignResourcesContentCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
