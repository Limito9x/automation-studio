using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Automation.Files.Infrastructure.Storage;

public class R2StorageService(IAmazonS3 s3Client, IOptions<R2Options> options) : IObjectStorageService
{
    private readonly R2Options _options = options.Value;

    public async Task<string> GeneratePresignedUploadUrlAsync(string storagePath, string contentType, TimeSpan expiration, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = storagePath,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiration),
            ContentType = contentType
        };

        return await s3Client.GetPreSignedURLAsync(request);
    }

    public async Task<long?> GetFileSizeAsync(string storagePath, CancellationToken ct = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _options.BucketName,
                Key = storagePath
            };

            var response = await s3Client.GetObjectMetadataAsync(request, ct);
            return response.ContentLength;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath)) return;

        var request = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = storagePath
        };

        await s3Client.DeleteObjectAsync(request, ct);
    }

    public async Task DeleteFilesAsync(IEnumerable<string> storagePaths, CancellationToken ct = default)
    {
        var paths = storagePaths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
        if (paths.Count == 0) return;

        // AWS SDK supports batch deleting up to 1000 objects at a time
        const int batchSize = 1000;
        for (var i = 0; i < paths.Count; i += batchSize)
        {
            var batch = paths.Skip(i).Take(batchSize).Select(p => new KeyVersion { Key = p }).ToList();
            
            var request = new DeleteObjectsRequest
            {
                BucketName = _options.BucketName,
                Objects = batch
            };

            await s3Client.DeleteObjectsAsync(request, ct);
        }
    }

    public async Task<List<string>> GetAllFilesAsync(CancellationToken ct = default)
    {
        var allFiles = new List<string>();
        var request = new ListObjectsV2Request
        {
            BucketName = _options.BucketName
        };

        ListObjectsV2Response response;
        do
        {
            response = await s3Client.ListObjectsV2Async(request, ct);
            allFiles.AddRange(response.S3Objects.Select(o => o.Key));
            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated == true);

        return allFiles;
    }

    public string GetPublicUrl(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            return string.Empty;
            
        var publicBaseUrl = _options.PublicBaseUrl?.TrimEnd('/');
        return $"{publicBaseUrl}/{storagePath}";
    }
}


