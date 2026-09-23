using Automation.Projects.Contracts;
using Automation.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Projects.Infrastructure.Services;

public class ProjectsApiService(ProjectsDbContext db) : IProjectsApi
{
    public async Task<Result<ProjectExecutorConfigResultDto?>> GetExecutorConfigAsync(
        Guid projectId,
        Guid agentId,
        string executorKey,
        CancellationToken ct = default
    )
    {
        var config = await db.ProjectExecutorConfigs
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.AgentId == agentId && x.ExecutorKey == executorKey)
            .Select(x => new ProjectExecutorConfigResultDto(
                x.Id,
                x.ProjectId,
                x.AgentId,
                x.ExecutorKey,
                x.Settings
            ))
            .FirstOrDefaultAsync(ct);

        return Result.Ok<ProjectExecutorConfigResultDto?>(config);
    }
}
