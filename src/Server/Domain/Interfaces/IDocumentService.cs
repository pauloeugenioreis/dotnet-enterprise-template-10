using ProjectTemplate.Shared.Models;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Service for document upload, download and deletion using cloud storage.
/// </summary>
public interface IDocumentService
{
    Task<DocumentUploadResponse> UploadAsync(string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType)> DownloadAsync(string fileName, CancellationToken cancellationToken = default);

    Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
}
