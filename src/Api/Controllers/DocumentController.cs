using Microsoft.AspNetCore.Mvc;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Api.Controllers;

/// <summary>
/// Document management endpoints — example of cloud storage usage.
/// Requires AddStorage&lt;Program&gt;() to be enabled in Program.cs.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class DocumentController(
    IDocumentService documentService,
    ILogger<DocumentController> logger) : ApiControllerBase
{
    /// <summary>
    /// Upload a document to cloud storage
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(DocumentUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
    public async Task<IActionResult> UploadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        logger.LogInformation("Upload request received for file {FileName}", file?.FileName);

        await using var stream = file!.OpenReadStream();
        var result = await documentService.UploadAsync(file.FileName, file.ContentType, stream, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Download a document from cloud storage
    /// </summary>
    [HttpGet("{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAsync(string fileName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Download request for {FileName}", fileName);

        var (stream, contentType) = await documentService.DownloadAsync(fileName, cancellationToken);

        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// Delete a document from cloud storage
    /// </summary>
    [HttpDelete("{fileName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(string fileName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Delete request for {FileName}", fileName);

        await documentService.DeleteAsync(fileName, cancellationToken);

        return NoContent();
    }
}
