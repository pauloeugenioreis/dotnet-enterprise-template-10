using ProjectTemplate.SharedModels;
using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// MongoDB repository interface for CustomerReview documents.
/// Extends IMongoRepository with filter-specific queries.
/// </summary>
public interface ICustomerReviewRepository : IMongoRepository<CustomerReview>
{
    Task<(IEnumerable<CustomerReview> Items, long Total)> GetByFilterAsync(
        string? productName,
        int? minRating,
        bool? isApproved,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);
}
