using Capstone.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Options;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace Capstone.Infrastructure.Services;

public class FileStorageProvider : IFileStorageProvider
{
    private readonly IAmazonS3 _s3Client;
    private readonly BucketSettings _bucketSettings;

    public FileStorageProvider(IAmazonS3 s3Client, IOptions<BucketSettings> bucketSettings)
    {
        _s3Client = s3Client;
        _bucketSettings = bucketSettings.Value;
    }

    public async Task<string> UploadImageAsync(string folder, string id, Stream fileStream, string contentType, string extension)
    {
        var key = $"{folder}/{id}{extension}";

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = key,
            BucketName = _bucketSettings.BucketName,
            ContentType = contentType
        };

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(uploadRequest);

        return key;
    }

    public async Task UploadUserImageAsync(Guid userId, Stream fileStream, string contentType, string extension)
    {
        var key = $"users/{userId}{extension}";

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = key,
            BucketName = _bucketSettings.BucketName,
            ContentType = contentType
        };

        var transferUtility = new TransferUtility(_s3Client);

        await transferUtility.UploadAsync(uploadRequest);
    }

    public Task<string> GetImageUrlAsync(string imageKey, int expirationInMinutes = 60)
    {
        var request = new Amazon.S3.Model.GetPreSignedUrlRequest {
            BucketName = _bucketSettings.BucketName,
            Key = imageKey,
            Expires = DateTime.UtcNow.AddHours(7).AddMinutes(expirationInMinutes)
        };

        var url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    public async Task DeleteImageAsync(string imageKey)
    {
        var request = new Amazon.S3.Model.DeleteObjectRequest {
            BucketName = _bucketSettings.BucketName,
            Key = imageKey
        };

        await _s3Client.DeleteObjectAsync(request);
    }
}