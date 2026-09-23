using Automation.Platform.Domain.Entities;
using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Features.PlatformExtensions.CreateExtensions;

public class CreateExtensionsHandler(PlatformDbContext db)
{
    public async Task<Result<IReadOnlyList<PlatformExtensionDto>>> HandleAsync(CreateExtensionsCommand command, CancellationToken ct)
    {
        var resultEntities = await EnsureExtensionsExistAsync(db, command.Extensions, ct);
        return Result.Ok<IReadOnlyList<PlatformExtensionDto>>(resultEntities.Adapt<List<PlatformExtensionDto>>());
    }

    public static async Task<List<PlatformExtension>> EnsureExtensionsExistAsync(PlatformDbContext db, IEnumerable<string> extensions, CancellationToken ct)
    {
        var formatted = extensions
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().TrimStart('.').ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (formatted.Count == 0)
            return [];

        var formattedWithDots = formatted.Select(x => "." + x).ToList();

        var existingEntities = await db.PlatformExtensions
            .Where(x => formatted.Contains(x.Extension) || formattedWithDots.Contains(x.Extension))
            .ToListAsync(ct);

        var existingCleanNames = existingEntities
            .Select(x => x.Extension.Trim().TrimStart('.').ToLowerInvariant())
            .ToHashSet();

        var newEntities = new List<PlatformExtension>();

        foreach (var ext in formatted)
        {
            if (!existingCleanNames.Contains(ext))
            {
                var newEntity = new PlatformExtension(ext);
                db.PlatformExtensions.Add(newEntity);
                newEntities.Add(newEntity);
            }
        }

        if (newEntities.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            existingEntities.AddRange(newEntities);
        }

        return existingEntities;
    }
}
