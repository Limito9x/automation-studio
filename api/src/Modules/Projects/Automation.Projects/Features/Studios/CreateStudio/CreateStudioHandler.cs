using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Projects.Features.Studios.CreateStudio;

[Transactional(typeof(ProjectsDbContext))]
public class CreateStudioHandler(ProjectsDbContext db)
{
    public async Task<Result<StudioDto>> HandleAsync(
        CreateStudioCommand command,
        CancellationToken ct)
    {
        var rawSlug = string.IsNullOrWhiteSpace(command.Slug) ? command.Name : command.Slug;
        var slug = rawSlug.Trim().ToLowerInvariant().Replace(" ", "-");

        var exists = await db.Studios.AnyAsync(x => x.Slug == slug, ct);
        if (exists)
        {
            return Result.Fail(new ConflictError($"Studio with slug '{slug}' already exists."));
        }

        var studio = command.Adapt<Studio>();
        studio.Slug = slug;

        db.Studios.Add(studio);
        await db.SaveChangesAsync(ct);

        return Result.Ok(studio.Adapt<StudioDto>());
    }
}
