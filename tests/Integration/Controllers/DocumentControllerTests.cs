using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;
using Xunit;

namespace ProjectTemplate.Integration.Tests.Controllers;

/// <summary>
/// Integration tests for DocumentController using an in-memory document service.
/// </summary>
public class DocumentControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public DocumentControllerTests(WebApplicationFactoryFixture factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDocumentService>();
                services.AddSingleton<IDocumentService, InMemoryDocumentService>();
            });
        });

        _client = customFactory.CreateClient();
    }

    [Fact]
    public async Task Upload_ValidFile_ReturnsOk()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        var fileBytes = "integration file content"u8.ToArray();
        using var streamContent = new ByteArrayContent(fileBytes);
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        content.Add(streamContent, "file", "integration.txt");

        // Act
        var response = await _client.PostAsync("/api/v1/Document/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<DocumentUploadResponse>();
        payload.Should().NotBeNull();
        payload!.FileName.Should().EndWith("integration.txt");
        payload.ContentType.Should().Be("text/plain");
    }

    [Fact]
    public async Task Download_ExistingFile_ReturnsFile()
    {
        // Arrange
        var uploadResult = await UploadTestFileAsync("download.txt", "download-content");

        // Act
        var response = await _client.GetAsync($"/api/v1/Document/{uploadResult.FileName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/plain");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("download-content");
    }

    [Fact]
    public async Task Delete_ExistingFile_ReturnsNoContent()
    {
        // Arrange
        var uploadResult = await UploadTestFileAsync("delete.txt", "delete-content");

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/v1/Document/{uploadResult.FileName}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<DocumentUploadResponse> UploadTestFileAsync(string fileName, string content)
    {
        using var multipart = new MultipartFormDataContent();
        using var streamContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(content));
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        multipart.Add(streamContent, "file", fileName);

        var uploadResponse = await _client.PostAsync("/api/v1/Document/upload", multipart);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await uploadResponse.Content.ReadFromJsonAsync<DocumentUploadResponse>();
        payload.Should().NotBeNull();

        return payload!;
    }

    private sealed class InMemoryDocumentService : IDocumentService
    {
        private readonly Dictionary<string, (byte[] Data, string ContentType)> _files = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new();

        public Task<DocumentUploadResponse> UploadAsync(string fileName, string contentType, Stream stream, CancellationToken cancellationToken = default)
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);

            var storedName = $"{Guid.NewGuid():N}_{fileName}";
            lock (_sync)
            {
                _files[storedName] = (memory.ToArray(), contentType);
            }

            return Task.FromResult(new DocumentUploadResponse
            {
                Url = $"https://local.test/{storedName}",
                FileName = storedName,
                ContentType = contentType,
                UploadedAt = DateTime.UtcNow
            });
        }

        public Task<(Stream Stream, string ContentType)> DownloadAsync(string fileName, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                if (!_files.TryGetValue(fileName, out var payload))
                {
                    throw new NotFoundException($"File '{fileName}' not found");
                }

                Stream stream = new MemoryStream(payload.Data);
                return Task.FromResult((stream, payload.ContentType));
            }
        }

        public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                var removed = _files.Remove(fileName);
                if (!removed)
                {
                    throw new NotFoundException($"File '{fileName}' not found");
                }
            }

            return Task.CompletedTask;
        }
    }
}
