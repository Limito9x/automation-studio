using Automation.Platform.Contracts;
using Automation.Platform.Domain.Entities;
using Automation.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Infrastructure;

public class PlatformApiService(PlatformDbContext db) : IPlatformApi
{
    public async Task<Result<IReadOnlyList<string>>> GetAllowedExtensionsAsync(
        Guid platformId,
        CancellationToken ct = default
    )
    {
        var platform = await db
            .Platforms.AsNoTracking()
            .Include(x => x.Extensions)
            .FirstOrDefaultAsync(x => x.Id == platformId, ct);

        if (platform is null)
            return Result.Ok<IReadOnlyList<string>>([]);

        var extensions = platform.Extensions
            .Select(x => x.Extension.Trim().TrimStart('.').ToLowerInvariant())
            .Distinct()
            .ToList();

        return Result.Ok<IReadOnlyList<string>>(extensions);
    }

    public async Task<Result<IReadOnlyList<string>>> GetAllowedExtensionsAsync(
        IEnumerable<Guid> platformIds,
        CancellationToken ct = default
    )
    {
        var ids = platformIds.Distinct().ToList();

        if (ids.Count == 0)
            return Result.Ok<IReadOnlyList<string>>([]);

        var rawExtensions = await db
            .Platforms.AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .SelectMany(p => p.Extensions)
            .Select(x => x.Extension)
            .ToListAsync(ct);

        var extensions = rawExtensions
            .Select(x => x.Trim().TrimStart('.').ToLowerInvariant())
            .Distinct()
            .ToList();

        return Result.Ok<IReadOnlyList<string>>(extensions);
    }

    public async Task<Result<Guid?>> GetExtensionIdAsync(
        Guid platformId,
        string extension,
        CancellationToken ct = default
    )
    {
        var extClean = extension.Trim().TrimStart('.').ToLowerInvariant();
        var extEntity = await db
            .PlatformExtensions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Extension == extClean || x.Extension == "." + extClean, ct);

        return Result.Ok(extEntity?.Id);
    }

    public async Task<Result<Dictionary<string, Guid>>> GetExtensionMapAsync(
        IEnumerable<Guid>? platformIds,
        CancellationToken ct = default
    )
    {
        IQueryable<PlatformExtension> query;

        if (platformIds != null && platformIds.Any())
        {
            var pIds = platformIds.Distinct().ToList();
            query = db.Platforms.AsNoTracking()
                .Where(p => pIds.Contains(p.Id))
                .SelectMany(p => p.Extensions)
                .Distinct();
        }
        else
        {
            query = db.PlatformExtensions.AsNoTracking();
        }

        var list = await query
            .Select(x => new { x.Extension, x.Id })
            .ToListAsync(ct);

        var map = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in list)
        {
            var cleanExt = item.Extension.Trim().TrimStart('.').ToLowerInvariant();
            map[cleanExt] = item.Id;
        }

        return Result.Ok(map);
    }
}
