namespace ProjectTemplate.Shared.Models;

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

public record DocumentResponseDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record CreateDocumentRequest
{
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}
