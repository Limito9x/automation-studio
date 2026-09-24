using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.Projects.CreateProject;

[Transactional(typeof(ProjectsDbContext))]
public class CreateProjectHandler(ProjectsDbContext db, ICurrentUserProvider userProvider)
{
    public async Task<Result<ProjectDto>> HandleAsync(
        CreateProjectCommand request,
        CancellationToken ct)
    {
        if (!userProvider.UserId.HasValue)
        {
            return Result.Fail<ProjectDto>(new UnauthorizedError("User is not authenticated"));
        }

        var project = request.Adapt<Project>();
        project.OwnerId = userProvider.UserId.Value;

        if (project.StudioId == Guid.Empty)
        {
            var defaultStudio = await db.Studios.FirstOrDefaultAsync(ct);
            if (defaultStudio is not null)
            {
                project.StudioId = defaultStudio.Id;
            }
        }

        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        
        return Result.Ok(project.Adapt<ProjectDto>());
    }
}
