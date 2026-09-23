using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.ProjectExecutorConfigs.GetProjectExecutorConfigs;

[NonTransactional]
public class GetProjectExecutorConfigsHandler(ProjectsDbContext db)
{
    public async Task<Result<IReadOnlyList<ProjectExecutorConfigDto>>> HandleAsync(
        GetProjectExecutorConfigsQuery query,
        CancellationToken ct)
    {
        var configs = await db.ProjectExecutorConfigs
            .AsNoTracking()
            .Where(x => x.ProjectId == query.ProjectId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProjectExecutorConfigDto(
                x.Id,
                x.ProjectId,
                x.AgentId,
                x.ExecutorKey,
                x.Settings,
                x.CreatedAt,
                x.UpdatedAt
            ))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<ProjectExecutorConfigDto>>(configs);
    }
}
