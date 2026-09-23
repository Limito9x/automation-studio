using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Resources.GetAvailableAgents;

public record GetAvailableAgentsQuery(List<Guid> ResourceIds, Guid WorkspaceId);

public record AvailableAgentDto(
    Guid AgentId,
    string AgentName,
    bool IsAvailable,
    List<string> AvailableExecutors,
    List<AgentResourceDto> AvailableResources
);

public record AgentResourceDto(Guid ResourceId, ResourceVersionDto? LatestVersion);
