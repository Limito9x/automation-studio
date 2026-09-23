using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Workspaces.GetWorkspaceById;

public record GetWorkspaceByIdQuery(Guid Id);

public record WorkspaceDetailDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    int AgentCount,
    int ResourceCount,
    int LocationCount,
    List<WorkspaceAgentDto> WorkspaceAgents,
    DateTimeOffset CreatedAt,
    IReadOnlyList<Guid>? PlatformIds = null
);