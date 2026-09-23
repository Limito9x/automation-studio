using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Projects.Features.Projects.DeleteProject;

public class DeleteProjectHandler(ProjectsDbContext db)
{
    public async Task<Result> HandleAsync(
        DeleteProjectCommand command,
        CancellationToken ct)
    {
        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (project is null) return Result.Fail(new NotFoundError("Project not found"));
        
        db.Projects.Remove(project);
        await db.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}

