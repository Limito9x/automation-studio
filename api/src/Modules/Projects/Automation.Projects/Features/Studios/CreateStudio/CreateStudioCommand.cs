namespace Automation.Projects.Features.Studios.CreateStudio;

public record CreateStudioCommand(
    string Name,
    string? Slug = null,
    string? Description = null
);
