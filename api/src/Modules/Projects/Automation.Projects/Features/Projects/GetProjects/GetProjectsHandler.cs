using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace Automation.Projects.Features.Projects.GetProjects;

public class GetProjectsHandler(ProjectsDbContext db, ICurrentUserProvider userProvider)
{
    public async Task<Result<PagedResult<ProjectDto>>> HandleAsync(
        GetProjectsQuery query,
        CancellationToken ct)
    {
        if (!userProvider.UserId.HasValue)
        {
            return Result.Fail(new UnauthorizedError("User is not authenticated"));
        }

        var userId = userProvider.UserId.Value;

        var mapper = new GridifyMapper<Project>()
            .GenerateMappings();

        var result = await db.Projects
            .Where(x => x.OwnerId == userId)
            .AsNoTracking()
            .ToPagedResultAsync<Project, ProjectDto>(query, mapper, ct);
            
        return result;
    }
}

