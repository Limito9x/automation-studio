using Automation.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.ProjectExecutorConfigs.DeleteProjectExecutorConfig;

[Transactional(typeof(ProjectsDbContext))]
public class DeleteProjectExecutorConfigHandler(ProjectsDbContext db)
{
    public async Task<Result> HandleAsync(
        DeleteProjectExecutorConfigCommand command,
        CancellationToken ct)
    {
        var config = await db.ProjectExecutorConfigs
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.ProjectId == command.ProjectId, ct);

        if (config == null)
        {
            return Result.Fail($"ProjectExecutorConfig with ID {command.Id} not found in project {command.ProjectId}.");
        }

        db.ProjectExecutorConfigs.Remove(config);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
