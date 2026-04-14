using Microsoft.Extensions.Logging;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Application.Services;

/// <summary>
/// Business logic service for CustomerReview (MongoDB).
/// Injects ICustomerReviewRepository — never the generic IMongoRepository.
/// </summary>
public class CustomerReviewService(
    ICustomerReviewRepository repository,
    ILogger<CustomerReviewService> logger) : ICustomerReviewService
{
    public async Task<CustomerReviewResponseDto?> GetReviewByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var review = await repository.GetByIdAsync(id, cancellationToken);
        return review is not null ? MapToResponse(review) : null;
    }

    public async Task<(IEnumerable<CustomerReviewResponseDto> Items, long Total)> GetAllReviewsAsync(
        string? productName,
        int? minRating,
        bool? isApproved,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var (reviews, total) = await repository.GetByFilterAsync(productName, minRating, isApproved, page, pageSize, cancellationToken);
        return (reviews.Select(MapToResponse).ToList(), total);
    }

    public async Task<CustomerReviewResponseDto> CreateReviewAsync(CreateCustomerReviewRequest dto, CancellationToken cancellationToken = default)
    {
        var review = new CustomerReview
        {
            ProductName = dto.ProductName,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            Rating = dto.Rating,
            Title = dto.Title,
            Comment = dto.Comment,
            Tags = dto.Tags,
            Metadata = dto.Metadata,
            IsVerifiedPurchase = dto.IsVerifiedPurchase,
            IsApproved = false
        };

        var created = await repository.AddAsync(review, cancellationToken);

        logger.LogInformation("CustomerReview created with ID {ReviewId} for product {ProductName}",
            created.Id, created.ProductName);

        return MapToResponse(created);
    }

    public async Task UpdateReviewAsync(string id, UpdateCustomerReviewRequest dto, CancellationToken cancellationToken = default)
    {
        var review = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"CustomerReview with ID {id} not found");

        review.ProductName = dto.ProductName;
        review.CustomerName = dto.CustomerName;
        review.CustomerEmail = dto.CustomerEmail;
        review.Rating = dto.Rating;
        review.Title = dto.Title;
        review.Comment = dto.Comment;
        review.Tags = dto.Tags;
        review.Metadata = dto.Metadata;
        review.IsVerifiedPurchase = dto.IsVerifiedPurchase;

        await repository.UpdateAsync(review, cancellationToken);

        logger.LogInformation("CustomerReview {ReviewId} updated", id);
    }

    public async Task ApproveReviewAsync(string id, ApproveCustomerReviewRequest dto, CancellationToken cancellationToken = default)
    {
        var review = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"CustomerReview with ID {id} not found");

        review.IsApproved = dto.IsApproved;

        await repository.UpdateAsync(review, cancellationToken);

        logger.LogInformation("CustomerReview {ReviewId} approval set to {IsApproved}", id, dto.IsApproved);
    }

    public async Task DeleteReviewAsync(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            throw new NotFoundException($"CustomerReview with ID {id} not found");
        }

        logger.LogInformation("CustomerReview {ReviewId} deleted", id);
    }

    private static CustomerReviewResponseDto MapToResponse(CustomerReview review) => new()
    {
        Id = review.Id,
        ProductName = review.ProductName,
        CustomerName = review.CustomerName,
        CustomerEmail = review.CustomerEmail,
        Rating = review.Rating,
        Title = review.Title,
        Comment = review.Comment,
        Tags = review.Tags,
        Metadata = review.Metadata,
        IsVerifiedPurchase = review.IsVerifiedPurchase,
        IsApproved = review.IsApproved,
        CreatedAt = review.CreatedAt,
        UpdatedAt = review.UpdatedAt
    };
}
