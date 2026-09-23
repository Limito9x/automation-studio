using Automation.Files.Contracts;
using Automation.Files.Domain.Entities;
using Automation.Files.Infrastructure.Persistence;
using Automation.Files.Infrastructure.Storage;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Automation.Files.Infrastructure;

public class AssetApiService(
    FilesDbContext dbContext,
    IObjectStorageService storageService,
    AssetRegistry assetRegistry
) : IAssetApi
{
    public async Task<Result<IReadOnlyList<AssetUploadDto>>> RequestUploadAsync(
        IEnumerable<UploadRequestItemDto> requests,
        CancellationToken ct = default
    )
    {
        var resultList = new List<AssetUploadDto>();
        var requestList = requests.ToList();
        if (requestList.Count == 0)
            return Result.Ok<IReadOnlyList<AssetUploadDto>>(resultList);

        var hashes = requestList.Select(x => x.HashSha256).Distinct().ToList();
        var existingAssets = await dbContext
            .Assets.Where(x => hashes.Contains(x.HashSha256))
            .ToListAsync(ct);

        var newAssets = new List<Asset>();
        var urlTasks = new List<(Asset asset, Task<string> urlTask)>();

        foreach (var req in requestList)
        {
            var existing = existingAssets.FirstOrDefault(x =>
                x.HashSha256 == req.HashSha256 && x.SizeBytes == req.SizeBytes
            );
            if (existing != null && existing.IsConfirmed)
            {
                resultList.Add(
                    new AssetUploadDto(
                        existing.Id,
                        req.HashSha256,
                        true,
                        null,
                        storageService.GetPublicUrl(existing.StoragePath)
                    )
                );
                continue;
            }

            var asset = existing;
            var dir1 = req.HashSha256.Substring(0, 2);
            var dir2 = req.HashSha256.Substring(2, 2);
            var storagePath = $"{dir1}/{dir2}/{req.HashSha256}{req.Extension}";

            if (asset == null)
            {
                asset = new Asset(
                    storagePath,
                    req.SizeBytes,
                    req.ContentType,
                    req.Extension,
                    req.HashSha256
                );
                newAssets.Add(asset);
                existingAssets.Add(asset); // Keep track in case there are duplicates in the request itself
            }

            var urlTask = storageService.GeneratePresignedUploadUrlAsync(
                storagePath,
                req.ContentType,
                TimeSpan.FromMinutes(15),
                ct
            );
            urlTasks.Add((asset, urlTask));
        }

        if (newAssets.Count > 0)
        {
            dbContext.Assets.AddRange(newAssets);
        }

        foreach (var tuple in urlTasks)
        {
            var url = await tuple.urlTask;
            resultList.Add(
                new AssetUploadDto(tuple.asset.Id, tuple.asset.HashSha256, false, url, null)
            );
        }

        if (newAssets.Count > 0)
        {
            await dbContext.SaveChangesAsync(ct);
        }

        return Result.Ok<IReadOnlyList<AssetUploadDto>>(resultList);
    }

    public async Task<Result<IReadOnlyList<ConfirmAssetDto>>> ConfirmUploadAsync(
        IEnumerable<Guid> assetIds,
        CancellationToken ct = default
    )
    {
        var idList = assetIds.ToList();
        if (idList.Count == 0)
            return Result.Ok<IReadOnlyList<ConfirmAssetDto>>(Array.Empty<ConfirmAssetDto>());

        var assets = await dbContext.Assets.Where(x => idList.Contains(x.Id)).ToListAsync(ct);

        var missingIds = idList.Except(assets.Select(x => x.Id)).ToList();
        if (missingIds.Count > 0)
        {
            return Result.Fail($"Assets not found: {string.Join(", ", missingIds)}");
        }

        var unconfirmedAssets = assets.Where(x => !x.IsConfirmed).ToList();
        if (unconfirmedAssets.Count == 0)
            return Result.Ok<IReadOnlyList<ConfirmAssetDto>>(Array.Empty<ConfirmAssetDto>()); // All already confirmed

        var verifyTasks = unconfirmedAssets.Select(async asset =>
        {
            var actualSize = await storageService.GetFileSizeAsync(asset.StoragePath, ct);
            if (actualSize == null)
                return Result.Fail($"File not found on storage for asset '{asset.Id}'.");

            if (actualSize.Value != asset.SizeBytes)
                return Result.Fail(
                    $"File size mismatch for asset '{asset.Id}'. Expected {asset.SizeBytes}, but found {actualSize.Value}."
                );

            asset.MarkAsConfirmed();
            return Result.Ok();
        });

        var results = await Task.WhenAll(verifyTasks);
        var errors = results.Where(r => r.IsFailed).SelectMany(r => r.Errors).ToList();
        if (errors.Count > 0)
            return Result.Fail(errors);

        await dbContext.SaveChangesAsync(ct);

        var resultAssets = unconfirmedAssets
            .Select(asset => new ConfirmAssetDto(
                asset.Id,
                asset.ContentType,
                asset.SizeBytes,
                storageService.GetPublicUrl(asset.StoragePath)
            ))
            .ToList();

        return Result.Ok<IReadOnlyList<ConfirmAssetDto>>(resultAssets);
    }

    public async Task<Result> VerifyAndLinkAsync(
        IEnumerable<AssetLinkRequestItem> items,
        string ownerEntityType,
        string slotKey,
        string ownerEntityId,
        int startSortOrder = 0,
        CancellationToken ct = default
    )
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
            return Result.Ok();

        var slotResult = assetRegistry.GetSlotOptions(ownerEntityType, slotKey);
        if (slotResult.IsFailed)
            return slotResult.ToResult();
        var options = slotResult.Value;

        var assetIds = itemList.Select(x => x.AssetId).ToList();
        var assets = await dbContext.Assets.Where(x => assetIds.Contains(x.Id)).ToListAsync(ct);

        var missingIds = assetIds.Except(assets.Select(x => x.Id)).ToList();
        if (missingIds.Count > 0)
            return Result.Fail($"Assets not found: {string.Join(", ", missingIds)}");

        foreach (var asset in assets)
        {
            if (!asset.IsConfirmed)
                return Result.Fail($"Asset '{asset.Id}' has not been confirmed.");

            if (asset.SizeBytes > options.MaxSizeBytes)
                return Result.Fail(
                    $"File size ({asset.SizeBytes} bytes) exceeds the maximum allowed size of {options.MaxSizeBytes} bytes for this slot."
                );

            if (options.AllowedContentTypes != null && options.AllowedContentTypes.Length > 0)
            {
                if (
                    !options.AllowedContentTypes.Contains(
                        asset.ContentType,
                        StringComparer.OrdinalIgnoreCase
                    )
                )
                    return Result.Fail(
                        $"Content type '{asset.ContentType}' is not allowed for this slot."
                    );
            }
        }

        var existingLinks = await dbContext
            .AssetLinks.Where(x =>
                x.OwnerEntityType == ownerEntityType
                && x.OwnerEntityId == ownerEntityId
                && x.SlotKey == slotKey
            )
            .ToListAsync(ct);

        if (!options.AllowMultiple)
        {
            if (itemList.Count > 1)
                return Result.Fail("This slot does not allow multiple files.");
            if (existingLinks.Count > 0)
            {
                dbContext.AssetLinks.RemoveRange(existingLinks);
            }
        }
        else if (
            options.MaxCount.HasValue
            && existingLinks.Count + itemList.Count > options.MaxCount.Value
        )
        {
            return Result.Fail(
                $"Cannot link more than {options.MaxCount.Value} files for this slot. Existing: {existingLinks.Count}, New: {itemList.Count}."
            );
        }

        // After removing existing links above, rebuild from the remaining ones
        var remainingExistingLinks = existingLinks
            .Where(l => !dbContext.Entry(l).State.Equals(EntityState.Deleted))
            .ToList();

        var newLinks = new List<AssetLink>();
        var currentSortOrder = startSortOrder;

        foreach (var item in itemList)
        {
            if (remainingExistingLinks.Any(x => x.AssetId == item.AssetId))
                continue;

            var link = new AssetLink(
                item.AssetId,
                ownerEntityType,
                slotKey,
                ownerEntityId,
                item.OriginalName,
                currentSortOrder++
            );
            newLinks.Add(link);
        }

        if (newLinks.Count > 0)
        {
            dbContext.AssetLinks.AddRange(newLinks);
            await dbContext.SaveChangesAsync(ct);
        }
        else if (existingLinks.Count > 0 && !options.AllowMultiple)
        {
            // Existing links were removed but no new ones added (same asset re-linked)
            await dbContext.SaveChangesAsync(ct);
        }

        return Result.Ok();
    }

    public async Task<Result> RemoveLinkAsync(
        Guid assetId,
        string ownerEntityId,
        CancellationToken ct = default
    )
    {
        var link = await dbContext.AssetLinks.FirstOrDefaultAsync(
            x => x.AssetId == assetId && x.OwnerEntityId == ownerEntityId,
            ct
        );

        if (link == null)
            return Result.Ok(); // idempotent: link already removed or not found

        dbContext.AssetLinks.Remove(link);
        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }

    public async Task<Result> RemoveLinkAsync(
        Guid assetId,
        string ownerEntityId,
        string ownerEntityType,
        string slotKey,
        CancellationToken ct = default
    )
    {
        var link = await dbContext.AssetLinks.FirstOrDefaultAsync(
            x =>
                x.AssetId == assetId
                && x.OwnerEntityId == ownerEntityId
                && x.OwnerEntityType == ownerEntityType
                && x.SlotKey == slotKey,
            ct
        );

        if (link == null)
            return Result.Ok();

        dbContext.AssetLinks.Remove(link);
        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }

    public async Task<Result> RemoveLinkAsync(
        string ownerEntityId,
        string ownerEntityType,
        string slotKey,
        CancellationToken ct = default
    )
    {
        var links = await dbContext
            .AssetLinks.Where(x =>
                x.OwnerEntityId == ownerEntityId
                && x.OwnerEntityType == ownerEntityType
                && x.SlotKey == slotKey
            )
            .ToListAsync(ct);

        if (links.Count == 0)
            return Result.Ok();

        dbContext.AssetLinks.RemoveRange(links);
        await dbContext.SaveChangesAsync(ct);

        return Result.Ok();
    }

    public async Task<Result<IReadOnlyList<AssetLinkDto>>> GetFilesAsync(
        string ownerEntityId,
        string ownerEntityType,
        string slotKey,
        CancellationToken ct = default
    )
    {
        var links = await dbContext
            .AssetLinks.Include(x => x.Asset)
            .Where(x =>
                x.OwnerEntityId == ownerEntityId
                && x.OwnerEntityType == ownerEntityType
                && x.SlotKey == slotKey
            )
            .OrderBy(x => x.SortOrder)
            .Select(x => new AssetLinkDto(
                x.Id,
                x.AssetId,
                storageService.GetPublicUrl(x.Asset.StoragePath),
                x.OriginalName,
                x.Asset.ContentType,
                x.Asset.SizeBytes,
                x.SortOrder,
                x.SlotKey,
                x.CreatedAt
            ))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<AssetLinkDto>>(links);
    }

    public async Task<Result<ILookup<string, AssetLinkDto>>> GetAllFilesForEntityAsync(
        string ownerEntityId,
        string ownerEntityType,
        CancellationToken ct = default
    )
    {
        var links = await dbContext
            .AssetLinks.Include(x => x.Asset)
            .Where(x => x.OwnerEntityId == ownerEntityId && x.OwnerEntityType == ownerEntityType)
            .OrderBy(x => x.SlotKey)
            .ThenBy(x => x.SortOrder)
            .Select(x => new AssetLinkDto(
                x.Id,
                x.AssetId,
                storageService.GetPublicUrl(x.Asset.StoragePath),
                x.OriginalName,
                x.Asset.ContentType,
                x.Asset.SizeBytes,
                x.SortOrder,
                x.SlotKey,
                x.CreatedAt
            ))
            .ToListAsync(ct);

        return Result.Ok(links.ToLookup(x => x.SlotKey));
    }

    public async Task<Result<IReadOnlyList<AssetDto>>> GetAssetsByIdsAsync(
        string ownerEntityType,
        string ownerEntityId,
        string slotKey,
        IEnumerable<string> assetIds,
        CancellationToken ct = default
    )
    {
        var assetLinks = await dbContext
            .AssetLinks.Include(x => x.Asset)
            .Where(x =>
                x.OwnerEntityId == ownerEntityId
                && x.OwnerEntityType == ownerEntityType
                && x.SlotKey == slotKey
                && assetIds.Contains(x.AssetId.ToString())
            )
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<AssetDto>>(
            assetLinks
                .Select(x => new AssetDto(
                    x.AssetId,
                    x.OriginalName,
                    x.Asset.ContentType,
                    x.Asset.SizeBytes,
                    storageService.GetPublicUrl(x.Asset.StoragePath)
                ))
                .ToList()
        );
    }

    public async Task<Result<AssetDto>> GetAssetByIdAsync(
        Guid assetId,
        CancellationToken ct = default
    )
    {
        var asset = await dbContext.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == assetId, ct);
        if (asset == null)
            return Result.Fail($"Asset with ID '{assetId}' was not found.");

        return Result.Ok(new AssetDto(
            asset.Id,
            System.IO.Path.GetFileName(asset.StoragePath),
            asset.ContentType,
            asset.SizeBytes,
            storageService.GetPublicUrl(asset.StoragePath)
        ));
    }

    public async Task<Result<IReadOnlyList<AssetDto>>> GetAssetsByIdsAsync(
        IEnumerable<Guid> assetIds,
        CancellationToken ct = default
    )
    {
        var idList = assetIds.Distinct().ToList();
        var assets = await dbContext.Assets.AsNoTracking().Where(x => idList.Contains(x.Id)).ToListAsync(ct);
        return Result.Ok<IReadOnlyList<AssetDto>>(
            assets.Select(asset => new AssetDto(
                asset.Id,
                System.IO.Path.GetFileName(asset.StoragePath),
                asset.ContentType,
                asset.SizeBytes,
                storageService.GetPublicUrl(asset.StoragePath)
            )).ToList()
        );
    }

    public async Task<Result<Dictionary<string, IReadOnlyList<AssetLinkDto>>>> GetFilesAsync(
        IEnumerable<string> ownerEntityIds,
        string ownerEntityType,
        string slotKey,
        CancellationToken ct = default
    )
    {
        var assetLinks = await dbContext
            .AssetLinks.Include(x => x.Asset)
            .Where(x =>
                ownerEntityIds.Contains(x.OwnerEntityId)
                && x.OwnerEntityType == ownerEntityType
                && x.SlotKey == slotKey
            )
            .OrderBy(x => x.OwnerEntityId)
            .ThenBy(x => x.SortOrder)
            .ToListAsync(ct);

        var result = new Dictionary<string, IReadOnlyList<AssetLinkDto>>();
        foreach (var link in assetLinks)
        {
            if (!result.ContainsKey(link.OwnerEntityId))
                result[link.OwnerEntityId] = new List<AssetLinkDto>();
            ((List<AssetLinkDto>)result[link.OwnerEntityId]).Add(
                new AssetLinkDto(
                    link.Id,
                    link.AssetId,
                    storageService.GetPublicUrl(link.Asset.StoragePath),
                    link.OriginalName,
                    link.Asset.ContentType,
                    link.Asset.SizeBytes,
                    link.SortOrder,
                    link.SlotKey,
                    link.CreatedAt
                )
            );
        }
        return Result.Ok(result);
    }

    public async Task<Result> UpsertMultipleAsync(
        string ownerEntityType,
        string ownerEntityId,
        string slotKey,
        IEnumerable<AssetUpsertDto> dtos,
        CancellationToken ct = default
    )
    {
        if (dtos == null || !dtos.Any())
            return Result.Ok();

        bool changed = false;

        var existing = await dbContext
            .AssetLinks.Where(x =>
                x.OwnerEntityId == ownerEntityId
                && x.OwnerEntityType == ownerEntityType
                && x.SlotKey == slotKey
            )
            .ToListAsync(ct);

        var assets = existing.Select(al => (al.AssetId, al.OriginalName)).ToHashSet();

        var options = assetRegistry.GetSlotOptions(ownerEntityType, slotKey);

        if (options == null || !options.Value.AllowMultiple)
        {
            return Result.Fail("This slot does not allow multiple files.");
        }

        var toAdd = dtos.Where(x => !assets.Contains((x.AssetId, x.Name))).ToList();

        var toRemove = existing.Where(x => !assets.Contains((x.AssetId, x.OriginalName))).ToList();

        if (toAdd.Count > 0)
        {
            var lastSortOrder = existing.Any() ? existing.Max(x => x.SortOrder) + 1 : 1;
            var newLinks = toAdd
                .Select(
                    (dto, idx) =>
                        new AssetLink(
                            dto.AssetId,
                            ownerEntityType,
                            slotKey,
                            ownerEntityId,
                            dto.Name,
                            lastSortOrder + idx
                        )
                )
                .ToList();
            dbContext.AssetLinks.AddRange(newLinks);

            changed = true;
        }

        if (toRemove.Count > 0)
        {
            dbContext.AssetLinks.RemoveRange(toRemove);
            changed = true;
        }

        if (changed)
        {
            await dbContext.SaveChangesAsync(ct);
        }

        return Result.Ok();
    }
}
