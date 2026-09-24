using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.Projects.GetProjectById;

[NonTransactional]
public class GetProjectByIdHandler(ProjectsDbContext db)
{
    public async Task<Result<ProjectDto>> HandleAsync(
        GetProjectByIdQuery query,
        CancellationToken ct)
    {
        var project = await db.Projects.FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (project is null) return Result.Fail(new NotFoundError("Project not found"));
        
        return Result.Ok(project.Adapt<ProjectDto>());
    }
}
