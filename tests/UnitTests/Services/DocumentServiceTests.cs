using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ProjectTemplate.Application.Services;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.UnitTests.Services;

public class DocumentServiceTests
{
    private readonly Mock<IStorageService> _mockStorage = new();
    private readonly Mock<ILogger<DocumentService>> _mockLogger = new();
    private readonly DocumentService _service;
    private const string DefaultBucket = "test-bucket";

    public DocumentServiceTests()
    {
        var settings = Options.Create(new AppSettings
        {
            EnvironmentName = "Testing",
            Infrastructure = new InfrastructureSettings
            {
                Storage = new StorageSettings { DefaultBucket = DefaultBucket }
            }
        });
        _service = new DocumentService(_mockStorage.Object, settings, _mockLogger.Object);
    }

    // ── UploadAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UploadAsync_WithEmptyStream_ThrowsBusinessException()
    {
        var emptyStream = new MemoryStream();

        var act = () => _service.UploadAsync("report.pdf", "application/pdf", emptyStream);

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task UploadAsync_WithValidStream_CallsStorageServiceWithCorrectBucket()
    {
        var stream = CreateStream();
        _mockStorage.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
            .ReturnsAsync("https://storage.example.com/file.pdf");

        await _service.UploadAsync("report.pdf", "application/pdf", stream);

        _mockStorage.Verify(s => s.UploadAsync(
            DefaultBucket, It.IsAny<string>(), "application/pdf", stream), Times.Once);
    }

    [Fact]
    public async Task UploadAsync_WithValidStream_GeneratesGuidPrefixedObjectName()
    {
        var stream = CreateStream();
        string? capturedObjectName = null;
        _mockStorage
            .Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
            .Callback<string, string, string, Stream>((_, name, _, _) => capturedObjectName = name)
            .ReturnsAsync("https://storage.example.com/file.pdf");

        await _service.UploadAsync("report.pdf", "application/pdf", stream);

        capturedObjectName.Should().NotBeNull();
        capturedObjectName.Should().EndWith("_report.pdf");
        // First part should be a valid GUID (32 hex chars, no hyphens since :N format)
        var prefix = capturedObjectName!.Split('_')[0];
        prefix.Should().HaveLength(32);
        Guid.TryParseExact(prefix, "N", out _).Should().BeTrue();
    }

    [Fact]
    public async Task UploadAsync_WithValidStream_ReturnsResponseWithCorrectContentType()
    {
        var stream = CreateStream();
        _mockStorage.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
            .ReturnsAsync("https://storage.example.com/file.pdf");

        var result = await _service.UploadAsync("report.pdf", "application/pdf", stream);

        result.ContentType.Should().Be("application/pdf");
        result.Url.Should().Be("https://storage.example.com/file.pdf");
    }

    // ── DownloadAsync — MIME type detection ─────────────────────────────────

    [Theory]
    [InlineData("file.pdf", "application/pdf")]
    [InlineData("file.jpg", "image/jpeg")]
    [InlineData("file.jpeg", "image/jpeg")]
    [InlineData("file.png", "image/png")]
    [InlineData("file.gif", "image/gif")]
    [InlineData("file.doc", "application/msword")]
    [InlineData("file.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("file.xls", "application/vnd.ms-excel")]
    [InlineData("file.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("file.csv", "text/csv")]
    [InlineData("file.txt", "text/plain")]
    [InlineData("file.zip", "application/zip")]
    [InlineData("file.unknown", "application/octet-stream")]
    public async Task DownloadAsync_ReturnsCorrectContentTypeForExtension(string fileName, string expectedContentType)
    {
        SetupDownloadCallback();

        var (_, contentType) = await _service.DownloadAsync(fileName);

        contentType.Should().Be(expectedContentType);
    }

    [Fact]
    public async Task DownloadAsync_WithValidFile_ResetsStreamToPositionZero()
    {
        SetupDownloadCallback(content: [0x01, 0x02, 0x03]);

        var (stream, _) = await _service.DownloadAsync("file.pdf");

        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task DownloadAsync_CallsStorageServiceWithCorrectBucketAndFileName()
    {
        SetupDownloadCallback();

        await _service.DownloadAsync("report.pdf");

        _mockStorage.Verify(s => s.DownloadAsync(DefaultBucket, "report.pdf", It.IsAny<Stream>()), Times.Once);
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_CallsStorageServiceWithCorrectBucketAndFileName()
    {
        _mockStorage.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        await _service.DeleteAsync("report.pdf");

        _mockStorage.Verify(s => s.DeleteAsync(DefaultBucket, "report.pdf"), Times.Once);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static MemoryStream CreateStream(byte[]? content = null)
    {
        var bytes = content ?? [0x25, 0x50, 0x44, 0x46]; // minimal PDF magic bytes
        return new MemoryStream(bytes);
    }

    private void SetupDownloadCallback(byte[]? content = null)
    {
        var bytes = content ?? [0x01];
        _mockStorage
            .Setup(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
            .Callback<string, string, Stream>((_, _, dest) => dest.Write(bytes))
            .Returns(Task.CompletedTask);
    }
}
