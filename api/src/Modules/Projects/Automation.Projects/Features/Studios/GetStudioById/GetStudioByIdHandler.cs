using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.Studios.GetStudioById;

[NonTransactional]
public class GetStudioByIdHandler(ProjectsDbContext db)
{
    public async Task<Result<StudioDto>> HandleAsync(
        GetStudioByIdQuery query,
        CancellationToken ct)
    {
        var studio = await db.Studios.FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (studio is null)
        {
            return Result.Fail(new NotFoundError("Studio not found"));
        }

        return Result.Ok(studio.Adapt<StudioDto>());
    }
}
