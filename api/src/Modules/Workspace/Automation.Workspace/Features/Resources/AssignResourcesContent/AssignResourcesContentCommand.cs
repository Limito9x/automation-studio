namespace Automation.Workspace.Features.Resources.AssignResourcesContent;

public record AssignResourcesContentCommand(
    List<Guid> ResourceIds,
    Guid? ContentId
);
