using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using ProjectTemplate.Domain.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProjectTemplate.Infrastructure.Services;

/// <summary>
/// Google Cloud Storage service implementation
/// Provides blob storage operations for file management
/// </summary>
public class StorageService : IStorageService
{
    private readonly StorageClient _storageClient;
    private readonly ILogger<StorageService> _logger;

    public StorageService(StorageClient storageClient, ILogger<StorageService> logger)
    {
        _storageClient = storageClient;
        _logger = logger;
    }

    public async Task<string> UploadAsync(string bucketName, string objectName, string contentType, Stream stream)
    {
        try
        {
            _logger.LogInformation("Uploading object {ObjectName} to bucket {BucketName}", objectName, bucketName);

            var obj = await _storageClient.UploadObjectAsync(
                bucket: bucketName,
                objectName: objectName,
                contentType: contentType,
                source: stream).ConfigureAwait(false);

            var publicUrl = GetPublicUrl(bucketName, objectName);
            _logger.LogInformation("Successfully uploaded object {ObjectName}. URL: {Url}", objectName, publicUrl);

            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading object {ObjectName} to bucket {BucketName}", objectName, bucketName);
            throw;
        }
    }

    public async Task DownloadAsync(string bucketName, string objectName, Stream destination)
    {
        try
        {
            _logger.LogInformation("Downloading object {ObjectName} from bucket {BucketName}", objectName, bucketName);

            await _storageClient.DownloadObjectAsync(
                bucket: bucketName,
                objectName: objectName,
                destination: destination).ConfigureAwait(false);

            _logger.LogInformation("Successfully downloaded object {ObjectName}", objectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading object {ObjectName} from bucket {BucketName}", objectName, bucketName);
            throw;
        }
    }

    public async Task DeleteAsync(string bucketName, string objectName)
    {
        try
        {
            _logger.LogInformation("Deleting object {ObjectName} from bucket {BucketName}", objectName, bucketName);

            await _storageClient.DeleteObjectAsync(bucket: bucketName, objectName: objectName).ConfigureAwait(false);

            _logger.LogInformation("Successfully deleted object {ObjectName}", objectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting object {ObjectName} from bucket {BucketName}", objectName, bucketName);
            throw;
        }
    }

    public string GetPublicUrl(string bucketName, string objectName)
    {
        return $"https://storage.googleapis.com/{bucketName}/{objectName}";
    }
}
