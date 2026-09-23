using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.ProjectExecutorConfigs.UpsertProjectExecutorConfig;

[Transactional(typeof(ProjectsDbContext))]
public class UpsertProjectExecutorConfigHandler(ProjectsDbContext db)
{
    public async Task<Result<ProjectExecutorConfigDto>> HandleAsync(
        UpsertProjectExecutorConfigCommand command,
        CancellationToken ct)
    {
        var projectExists = await db.Projects.AnyAsync(x => x.Id == command.ProjectId, ct);
        if (!projectExists)
        {
            return Result.Fail<ProjectExecutorConfigDto>($"Project with ID {command.ProjectId} not found.");
        }

        var config = await db.ProjectExecutorConfigs
            .FirstOrDefaultAsync(x => x.ProjectId == command.ProjectId &&
                                      x.AgentId == command.AgentId &&
                                      x.ExecutorKey == command.ExecutorKey, ct);

        if (config == null)
        {
            config = new ProjectExecutorConfig(
                command.ProjectId,
                command.AgentId,
                command.ExecutorKey,
                command.Settings
            );
            db.ProjectExecutorConfigs.Add(config);
        }
        else
        {
            config.Update(command.Settings);
        }

        await db.SaveChangesAsync(ct);

        return Result.Ok(new ProjectExecutorConfigDto(
            config.Id,
            config.ProjectId,
            config.AgentId,
            config.ExecutorKey,
            config.Settings,
            config.CreatedAt,
            config.UpdatedAt
        ));
    }
}
