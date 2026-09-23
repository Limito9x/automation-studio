using Automation.Files.Contracts;
using FluentResults;

using Automation.Files.Infrastructure.Persistence;
using Wolverine.Attributes;

namespace Automation.Files.Features.Assets.RequestUpload;

[Transactional(typeof(FilesDbContext))]
public class RequestUploadHandler(IAssetApi assetApi)
{
    public async Task<Result<IReadOnlyList<AssetUploadDto>>> HandleAsync(RequestUploadCommand command, CancellationToken ct)
    {
        return await assetApi.RequestUploadAsync(command.Items, ct);
    }
}



