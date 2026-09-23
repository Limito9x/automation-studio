namespace Automation.Workspace.Features.Workspaces.CreateWorkspace;

public class CreateWorkspaceValidator : Validator<CreateWorkspaceCommand>
{
    public CreateWorkspaceValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

