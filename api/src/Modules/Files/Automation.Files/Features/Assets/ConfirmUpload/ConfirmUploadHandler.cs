using Automation.Files.Contracts;
using FluentResults;

using Automation.Files.Infrastructure.Persistence;
using Wolverine.Attributes;

namespace Automation.Files.Features.Assets.ConfirmUpload;

[Transactional(typeof(FilesDbContext))]
public class ConfirmUploadHandler(IAssetApi assetApi)
{
    public async Task<Result<IReadOnlyList<ConfirmAssetDto>>> HandleAsync(ConfirmUploadCommand command, CancellationToken ct)
    {
        return await assetApi.ConfirmUploadAsync(command.AssetIds, ct);
    }
}



