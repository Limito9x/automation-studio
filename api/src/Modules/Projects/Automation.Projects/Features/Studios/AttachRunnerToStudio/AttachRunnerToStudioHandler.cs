using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.Studios.AttachRunnerToStudio;

[Transactional(typeof(ProjectsDbContext))]
public class AttachRunnerToStudioHandler(ProjectsDbContext db)
{
    public async Task<Result<StudioRunnerDto>> HandleAsync(
        AttachRunnerToStudioCommand command,
        CancellationToken ct)
    {
        var studioExists = await db.Studios.AnyAsync(x => x.Id == command.StudioId, ct);
        if (!studioExists)
        {
            return Result.Fail(new NotFoundError($"Studio with ID '{command.StudioId}' was not found."));
        }

        var existing = await db.StudioRunners
            .FirstOrDefaultAsync(x => x.StudioId == command.StudioId && x.RunnerId == command.RunnerId, ct);

        if (existing is not null)
        {
            command.Adapt(existing);
            await db.SaveChangesAsync(ct);
            return Result.Ok(existing.Adapt<StudioRunnerDto>());
        }

        var runner = command.Adapt<StudioRunner>();
        db.StudioRunners.Add(runner);
        await db.SaveChangesAsync(ct);

        return Result.Ok(runner.Adapt<StudioRunnerDto>());
    }
}
