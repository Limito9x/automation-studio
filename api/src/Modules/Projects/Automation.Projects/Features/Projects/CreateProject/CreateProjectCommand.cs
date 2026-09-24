namespace Automation.Projects.Features.Projects.CreateProject;

public record CreateProjectCommand(string Name, Guid? StudioId = null);
