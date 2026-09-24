namespace Automation.Projects.Features.Studios.AttachRunnerToStudio;

public record AttachRunnerToStudioCommand(
    Guid StudioId,
    Guid RunnerId,
    string? Alias = null,
    bool IsApproved = true
);
