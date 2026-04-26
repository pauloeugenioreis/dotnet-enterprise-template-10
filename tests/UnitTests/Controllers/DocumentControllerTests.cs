using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Api.Controllers;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.UnitTests.Controllers;

public class DocumentControllerTests
{
    private readonly Mock<IDocumentService> _mockDocumentService = new();
    private readonly Mock<ILogger<DocumentController>> _mockLogger = new();
    private readonly DocumentController _controller;

    public DocumentControllerTests()
    {
        _controller = new DocumentController(_mockDocumentService.Object, _mockLogger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ── UploadAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UploadAsync_WithValidFile_ReturnsOkWithUploadResponse()
    {
        var file = CreateFormFile("test.pdf", "application/pdf", [1, 2, 3]);
        var uploadResponse = new DocumentUploadResponse
        {
            FileName = "test.pdf",
            Url = "https://storage.example.com/guid-test.pdf",
            ContentType = "application/pdf"
        };
        _mockDocumentService
            .Setup(s => s.UploadAsync("test.pdf", "application/pdf", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadResponse);

        var result = await _controller.UploadAsync(file, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(uploadResponse);
    }

    [Fact]
    public async Task UploadAsync_DelegatesFileNameAndContentTypeToService()
    {
        var file = CreateFormFile("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", [10, 20]);
        _mockDocumentService
            .Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DocumentUploadResponse());

        await _controller.UploadAsync(file, CancellationToken.None);

        _mockDocumentService.Verify(s => s.UploadAsync(
            "document.docx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── DownloadAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DownloadAsync_WithExistingFile_ReturnsFileStreamResult()
    {
        var stream = new MemoryStream([1, 2, 3]);
        _mockDocumentService
            .Setup(s => s.DownloadAsync("report.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync((stream, "application/pdf"));

        var result = await _controller.DownloadAsync("report.pdf", CancellationToken.None);

        var fileResult = result.Should().BeOfType<FileStreamResult>().Subject;
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileDownloadName.Should().Be("report.pdf");
    }

    [Fact]
    public async Task DownloadAsync_SetsFileDownloadNameToRequestedFileName()
    {
        var stream = new MemoryStream([42]);
        _mockDocumentService
            .Setup(s => s.DownloadAsync("image.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync((stream, "image/png"));

        var result = await _controller.DownloadAsync("image.png", CancellationToken.None);

        var fileResult = result.Should().BeOfType<FileStreamResult>().Subject;
        fileResult.FileDownloadName.Should().Be("image.png");
        fileResult.ContentType.Should().Be("image/png");
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WithExistingFile_ReturnsNoContent()
    {
        _mockDocumentService
            .Setup(s => s.DeleteAsync("old.pdf", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteAsync("old.pdf", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAsync_DelegatesFileNameToService()
    {
        _mockDocumentService
            .Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _controller.DeleteAsync("file.xlsx", CancellationToken.None);

        _mockDocumentService.Verify(s => s.DeleteAsync("file.xlsx", It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IFormFile CreateFormFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.Length).Returns(content.Length);
        return mockFile.Object;
    }
}
