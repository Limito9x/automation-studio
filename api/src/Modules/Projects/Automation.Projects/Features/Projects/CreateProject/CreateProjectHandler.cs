using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;

namespace Automation.Projects.Features.Projects.CreateProject;

public class CreateProjectHandler(ProjectsDbContext db, ICurrentUserProvider userProvider)
{
    public async Task<Result<ProjectDto>> HandleAsync(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (!userProvider.UserId.HasValue)
        {
            return Result.Fail<ProjectDto>("User is not authenticated");
        }

        var project = new Project(request.Name, userProvider.UserId.Value);
        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(new ProjectDto(project.Id, project.Name));
    }
}

