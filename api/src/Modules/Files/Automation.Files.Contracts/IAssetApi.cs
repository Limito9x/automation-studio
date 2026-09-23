using FluentResults;

namespace Automation.Files.Contracts;

public interface IAssetApi
{
    // Request Upload Multiple
    Task<Result<IReadOnlyList<AssetUploadDto>>> RequestUploadAsync(
        IEnumerable<UploadRequestItemDto> requests,
        CancellationToken ct = default
    );

    // Confirm Upload Multiple
    Task<Result<IReadOnlyList<ConfirmAssetDto>>> ConfirmUploadAsync(
        IEnumerable<Guid> assetIds,
        CancellationToken ct = default
    );

    // Verify And Link Multiple
    Task<Result> VerifyAndLinkAsync(
        IEnumerable<AssetLinkRequestItem> items,
        string ownerEntityType,
        string slotKey,
        string ownerEntityId,
        int startSortOrder = 0,
        CancellationToken ct = default
    );

    // Overload cho 1 file
    async Task<Result> VerifyAndLinkAsync(
        Guid assetId,
        string ownerEntityType,
        string slotKey,
        string ownerEntityId,
        string originalName,
        int sortOrder = 0,
        CancellationToken ct = default
    ) =>
        await VerifyAndLinkAsync(
            new[] { new AssetLinkRequestItem(assetId, originalName) },
            ownerEntityType,
            slotKey,
            ownerEntityId,
            sortOrder,
            ct
        );

    // Xóa link theo Asset ID và Owner Entity ID
    Task<Result> RemoveLinkAsync(
        Guid assetId,
        string ownerEntityId,
        CancellationToken ct = default
    );

    // Xóa link cụ thể theo Asset ID, Slot Key và Owner Entity
    Task<Result> RemoveLinkAsync(
        Guid assetId,
        string ownerEntityId,
        string ownerEntityType,
        string slotKey,
        CancellationToken ct = default
    );

    // Xóa tất cả link trong 1 Slot của Owner Entity (clear slot)
    Task<Result> RemoveLinkAsync(
        string ownerEntityId,
        string ownerEntityType,
        string slotKey,
        CancellationToken ct = default
    );

    // Query file theo slot
    Task<Result<IReadOnlyList<AssetLinkDto>>> GetFilesAsync(
        string ownerEntityId,
        string ownerEntityType,
        string slotKey,
        CancellationToken ct = default
    );

    // Query toàn bộ file của một Entity
    Task<Result<ILookup<string, AssetLinkDto>>> GetAllFilesForEntityAsync(
        string ownerEntityId,
        string ownerEntityType,
        CancellationToken ct = default
    );

    // Query file theo Asset IDs
    Task<Result<IReadOnlyList<AssetDto>>> GetAssetsByIdsAsync(
        string ownerEntityType,
        string ownerEntityId,
        string slotKey,
        IEnumerable<string> assetIds,
        CancellationToken ct = default
    );

    // Query asset trực tiếp theo Asset ID độc lập
    Task<Result<AssetDto>> GetAssetByIdAsync(
        Guid assetId,
        CancellationToken ct = default
    );

    Task<Result<IReadOnlyList<AssetDto>>> GetAssetsByIdsAsync(
        IEnumerable<Guid> assetIds,
        CancellationToken ct = default
    );

    // Query trả về dictionary với cùng entity nhưng nhiều id
    Task<Result<Dictionary<string, IReadOnlyList<AssetLinkDto>>>> GetFilesAsync(
        IEnumerable<string> ownerEntityIds,
        string ownerEntityType,
        string slotKey,
        CancellationToken ct = default
    );

    // Upsert nhiều file theo slot
    Task<Result> UpsertMultipleAsync(
        string ownerEntityType,
        string ownerEntityId,
        string slotKey,
        IEnumerable<AssetUpsertDto> dtos,
        CancellationToken ct = default
    );
}
