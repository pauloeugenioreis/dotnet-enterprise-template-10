using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Api.Controllers;

/// <summary>
/// CustomerReview CRUD endpoints — MongoDB document store example.
/// Demonstrates the complete MongoDB integration pattern for this template.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CustomerReviewController(
    ICustomerReviewService reviewService,
    ILogger<CustomerReviewController> logger) : ApiControllerBase
{
    /// <summary>
    /// Get all reviews with optional filters and pagination.
    /// </summary>
    [HttpGet]

    [ProducesResponseType(typeof(PagedResponse<CustomerReviewResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<CustomerReviewResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] string? productName,
        [FromQuery] int? minRating,
        [FromQuery] bool? isApproved,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var (items, total) = await reviewService.GetAllReviewsAsync(
            productName, minRating, isApproved, page, pageSize, cancellationToken);

        if (page.HasValue && pageSize.HasValue)
        {
            return HandlePagedResult(items, total, page.Value, pageSize.Value);
        }

        return Ok(items);
    }

    /// <summary>
    /// Get a review by its MongoDB ObjectId.
    /// </summary>
    [HttpGet("{id}")]

    [ProducesResponseType(typeof(CustomerReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var review = await reviewService.GetReviewByIdAsync(id, cancellationToken);

        if (review is null)
        {
            logger.LogWarning("CustomerReview {ReviewId} not found", id);
            return NotFound(new { message = $"CustomerReview with ID {id} not found" });
        }

        return Ok(review);
    }

    /// <summary>
    /// Create a new customer review.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerReviewResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCustomerReviewRequest dto,
        CancellationToken cancellationToken)
    {
        var created = await reviewService.CreateReviewAsync(dto, cancellationToken);
        return Created($"/api/v1/CustomerReview/{created.Id}", created);
    }

    /// <summary>
    /// Update an existing customer review.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        string id,
        [FromBody] UpdateCustomerReviewRequest dto,
        CancellationToken cancellationToken)
    {
        await reviewService.UpdateReviewAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Approve or reject a customer review.
    /// </summary>
    [HttpPatch("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveAsync(
        string id,
        [FromBody] ApproveCustomerReviewRequest dto,
        CancellationToken cancellationToken)
    {
        await reviewService.ApproveReviewAsync(id, dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete a customer review.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await reviewService.DeleteReviewAsync(id, cancellationToken);
        return NoContent();
    }
}
