using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.Projects.UpdateProject;

[Transactional(typeof(ProjectsDbContext))]
public class UpdateProjectHandler(ProjectsDbContext db)
{
    public async Task<Result<ProjectDto>> HandleAsync(
        UpdateProjectCommand request,
        CancellationToken ct)
    {
        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (project is null) return Result.Fail(new NotFoundError("Project not found"));
        
        request.Adapt(project);
        await db.SaveChangesAsync(ct);
        
        return Result.Ok(project.Adapt<ProjectDto>());
    }
}
