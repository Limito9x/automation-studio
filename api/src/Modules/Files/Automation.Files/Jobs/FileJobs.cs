using Automation.Files.Infrastructure.Persistence;
using Automation.Files.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Automation.Files.Jobs;

public class FileJobs(
    FilesDbContext dbContext,
    IObjectStorageService fileStorageService,
    ILogger<FileJobs> logger)
{
    private const int BatchSize = 1000;

    public async Task CleanupOrphanedFiles(CancellationToken ct = default)
    {
        logger.LogInformation("Start cleaning up (Orphaned Files)...");

        var thresholdDate = DateTimeOffset.UtcNow.AddHours(-24);
        var totalDeleted = 0;
        var lastId = Guid.Empty;

        while (!ct.IsCancellationRequested)
        {
            var batch = await dbContext.Assets
                .IgnoreQueryFilters()
                .Where(f => f.Id > lastId)
                .Where(f => f.CreatedAt < thresholdDate)
                .Where(f => !dbContext.AssetLinks.Any(l => l.AssetId == f.Id))
                .OrderBy(f => f.Id)
                .Take(BatchSize)
                .Select(f => new { f.Id, f.StoragePath })
                .ToListAsync(ct);

            if (batch.Count == 0)
            {
                break;
            }

            logger.LogInformation("Processing batch {Count} of orphaned files. Cursor Id: {LastId}", batch.Count, lastId);

            var pathsToDelete = batch.Select(x => x.StoragePath).ToList();
            var idsToDelete = batch.Select(x => x.Id).ToList();

            try
            {
                await fileStorageService.DeleteFilesAsync(pathsToDelete, ct);

                var deletedInDb = await dbContext.Assets
                    .IgnoreQueryFilters()
                    .Where(f => idsToDelete.Contains(f.Id))
                    .ExecuteDeleteAsync(ct);

                totalDeleted += deletedInDb;
                lastId = batch.Last().Id;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during batch cleanup. Stopping.");
                throw;
            }
        }

        logger.LogInformation("Cleanup complete. Total orphaned files hard deleted: {TotalDeleted}", totalDeleted);
    }

    public async Task CleanupUntrackedStorageFiles(CancellationToken ct = default)
    {
        logger.LogInformation("Start cleaning up (Untracked Storage Files)...");

        var allStorageFiles = await fileStorageService.GetAllFilesAsync(ct);
        var untrackedFiles = new HashSet<string>(allStorageFiles);
        
        logger.LogInformation("Found a total of {Count} files on Storage.", untrackedFiles.Count);

        if (untrackedFiles.Count == 0)
        {
            return;
        }

        var lastId = Guid.Empty;
        var totalDbFilesProcessed = 0;

        while (!ct.IsCancellationRequested)
        {
            var batch = await dbContext.Assets
                .IgnoreQueryFilters()
                .Where(f => f.Id > lastId)
                .OrderBy(f => f.Id)
                .Take(BatchSize)
                .Select(f => new { f.Id, f.StoragePath })
                .ToListAsync(ct);

            if (batch.Count == 0)
            {
                break;
            }

            foreach (var asset in batch)
            {
                untrackedFiles.Remove(asset.StoragePath);
            }

            totalDbFilesProcessed += batch.Count;
            lastId = batch.Last().Id;
        }

        logger.LogInformation("Finished comparing with {Count} files in the Database.", totalDbFilesProcessed);

        if (untrackedFiles.Count > 0)
        {
            logger.LogInformation("Detected {Count} ghost files on Storage. Starting deletion...", untrackedFiles.Count);

            try
            {
                await fileStorageService.DeleteFilesAsync(untrackedFiles, ct);
                logger.LogInformation("Successfully deleted {Count} ghost files.", untrackedFiles.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting ghost files on Storage.");
                throw;
            }
        }
        else
        {
            logger.LogInformation("No ghost files detected. Everything is synchronized.");
        }
    }
}


