using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.Studios.GetStudios;

[NonTransactional]
public class GetStudiosHandler(ProjectsDbContext db)
{
    public async Task<Result<IReadOnlyList<StudioDto>>> HandleAsync(
        GetStudiosQuery query,
        CancellationToken ct)
    {
        var studios = await db.Studios
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ProjectToType<StudioDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<StudioDto>>(studios);
    }
}
