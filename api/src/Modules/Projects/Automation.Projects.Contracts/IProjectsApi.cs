using System.Text.Json;
using FluentResults;

namespace Automation.Projects.Contracts;

public record ProjectExecutorConfigResultDto(
    Guid Id,
    Guid ProjectId,
    Guid AgentId,
    string ExecutorKey,
    JsonDocument? Settings
);

public interface IProjectsApi
{
    Task<Result<ProjectExecutorConfigResultDto?>> GetExecutorConfigAsync(
        Guid projectId,
        Guid agentId,
        string executorKey,
        CancellationToken ct = default
    );
}
