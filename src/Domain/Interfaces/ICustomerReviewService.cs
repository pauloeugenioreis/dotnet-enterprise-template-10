using ProjectTemplate.Domain.Dtos;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Service interface for CustomerReview business logic.
/// Does not inherit IService&lt;T&gt; because MongoDB entities use string Id
/// and a separate repository contract (IMongoRepository).
/// </summary>
public interface ICustomerReviewService
{
    Task<CustomerReviewResponseDto?> GetReviewByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<CustomerReviewResponseDto> Items, long Total)> GetAllReviewsAsync(
        string? productName,
        int? minRating,
        bool? isApproved,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);

    Task<CustomerReviewResponseDto> CreateReviewAsync(CreateCustomerReviewRequest dto, CancellationToken cancellationToken = default);
    Task UpdateReviewAsync(string id, UpdateCustomerReviewRequest dto, CancellationToken cancellationToken = default);
    Task ApproveReviewAsync(string id, ApproveCustomerReviewRequest dto, CancellationToken cancellationToken = default);
    Task DeleteReviewAsync(string id, CancellationToken cancellationToken = default);
}
