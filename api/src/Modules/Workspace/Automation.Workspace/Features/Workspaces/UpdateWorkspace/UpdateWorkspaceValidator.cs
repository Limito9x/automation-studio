namespace Automation.Workspace.Features.Workspaces.UpdateWorkspace;

public class UpdateWorkspaceValidator : Validator<UpdateWorkspaceCommand>
{
    public UpdateWorkspaceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

