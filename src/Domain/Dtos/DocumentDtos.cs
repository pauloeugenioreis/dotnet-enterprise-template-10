namespace ProjectTemplate.Domain.Dtos;

/// <summary>
/// Response returned after a successful document upload.
/// </summary>
public record DocumentUploadResponse
{
    public string Url { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
}
