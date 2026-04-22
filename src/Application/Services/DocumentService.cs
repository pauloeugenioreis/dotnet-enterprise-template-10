using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectTemplate.Domain;
using ProjectTemplate.SharedModels;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Application.Services;

/// <summary>
/// Document service that delegates storage operations to the configured cloud provider.
/// Requires AddStorage&lt;Program&gt;() to be enabled in Program.cs.
/// </summary>
public class DocumentService(
    IStorageService storageService,
    IOptions<AppSettings> options,
    ILogger<DocumentService> logger) : IDocumentService
{
    private string DefaultBucket => options.Value.Infrastructure.Storage.DefaultBucket;

    public async Task<DocumentUploadResponse> UploadAsync(string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream.Length == 0)
            throw new BusinessException("O arquivo enviado está vazio.");

        var objectName = $"{Guid.NewGuid():N}_{fileName}";

        logger.LogInformation("Uploading document {FileName} ({ContentType}, {Size} bytes)",
            objectName, contentType, stream.Length);

        var url = await storageService.UploadAsync(DefaultBucket, objectName, contentType, stream);

        logger.LogInformation("Document {FileName} uploaded successfully to {Url}", objectName, url);

        return new DocumentUploadResponse
        {
            Url = url,
            FileName = objectName,
            ContentType = contentType,
            UploadedAt = DateTime.UtcNow
        };
    }

    public async Task<(Stream Stream, string ContentType)> DownloadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Downloading document {FileName}", fileName);

        var memoryStream = new MemoryStream();
        await storageService.DownloadAsync(DefaultBucket, fileName, memoryStream);
        memoryStream.Position = 0;

        var contentType = GetContentType(fileName);

        return (memoryStream, contentType);
    }

    public async Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Deleting document {FileName}", fileName);

        await storageService.DeleteAsync(DefaultBucket, fileName);

        logger.LogInformation("Document {FileName} deleted successfully", fileName);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".csv" => "text/csv",
            ".txt" => "text/plain",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
