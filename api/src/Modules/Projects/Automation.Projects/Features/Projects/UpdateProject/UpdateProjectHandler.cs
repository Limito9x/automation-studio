using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Projects.Features.Projects.UpdateProject;

public class UpdateProjectHandler(ProjectsDbContext db)
{
    public async Task<Result<ProjectDto>> HandleAsync(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (project is null) return Result.Fail(new NotFoundError("Project not found"));
        
        project.Update(request.Name);
        await db.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(new ProjectDto(project.Id, project.Name));
    }
}

