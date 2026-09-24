using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.Studios.GetStudioRunners;

[NonTransactional]
public class GetStudioRunnersHandler(ProjectsDbContext db)
{
    public async Task<Result<IReadOnlyList<StudioRunnerDto>>> HandleAsync(
        GetStudioRunnersQuery query,
        CancellationToken ct)
    {
        var runners = await db.StudioRunners
            .AsNoTracking()
            .Where(x => x.StudioId == query.StudioId)
            .OrderBy(x => x.CreatedAt)
            .ProjectToType<StudioRunnerDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<StudioRunnerDto>>(runners);
    }
}
