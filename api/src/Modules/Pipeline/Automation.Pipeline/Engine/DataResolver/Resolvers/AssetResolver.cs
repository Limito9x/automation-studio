using Automation.Files.Contracts;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Engine.DataResolver.Resolvers;

public class AssetResolver(IAssetApi assetApi, ILogger<AssetResolver> logger)
{
    public async Task<object?> ResolveAssetIfApplicableAsync(object? value, CancellationToken ct = default)
    {
        if (value == null) return null;

        var strVal = value.ToString()?.Trim();
        if (!string.IsNullOrEmpty(strVal) && Guid.TryParse(strVal, out var assetGuid))
        {
            try
            {
                var assetResult = await assetApi.GetAssetByIdAsync(assetGuid, ct);
                if (assetResult.IsSuccess && assetResult.Value != null)
                {
                    var asset = assetResult.Value;
                    logger.LogInformation("Resolved Asset [{AssetId}] -> PublicUrl: {Url} (Filename: {FileName})",
                        assetGuid, asset.PublicUrl, asset.Name);

                    return new Dictionary<string, object?>
                    {
                        {
                            "$file", new Dictionary<string, object?>
                            {
                                { "url", asset.PublicUrl },
                                { "filename", asset.Name },
                                { "hash", asset.Id.ToString("N") },
                                { "size", asset.Size }
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to resolve asset ID '{AssetId}'", assetGuid);
            }
        }

        return value;
    }
}
