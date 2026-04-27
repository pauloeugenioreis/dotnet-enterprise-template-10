using BlazorApp.Client.Services.Base;
using ProjectTemplate.Shared.Models;
using System.Net.Http.Json;

namespace BlazorApp.Client.Services;

public interface IDocumentService
{
    Task<DocumentUploadResponse> UploadAsync(string fileName, string contentType, Stream stream, CancellationToken ct = default);
    Task<(Stream Stream, string ContentType)> DownloadAsync(string fileName, CancellationToken ct = default);
    Task DeleteAsync(string fileName, CancellationToken ct = default);
}

public class DocumentService(HttpClient http) 
    : BaseService(http, "api/v1/document"), IDocumentService
{
    public async Task<DocumentUploadResponse> UploadAsync(string fileName, string contentType, Stream stream, CancellationToken ct = default)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var response = await Http.PostAsync($"{ResourcePath}/upload", content, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DocumentUploadResponse>(cancellationToken: ct) ?? new();
    }

    public async Task<(Stream Stream, string ContentType)> DownloadAsync(string fileName, CancellationToken ct = default)
    {
        var response = await Http.GetAsync($"{ResourcePath}/{fileName}", ct);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(ct);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        return (stream, contentType);
    }

    public async Task DeleteAsync(string fileName, CancellationToken ct = default)
    {
        await DeleteRequestAsync($"{ResourcePath}/{fileName}", ct);
    }
}
