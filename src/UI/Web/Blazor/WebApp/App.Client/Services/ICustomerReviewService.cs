using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services;

public interface ICustomerReviewService
{
    Task<PagedResponse<CustomerReviewResponseDto>> GetReviewsAsync(int page = 1, int pageSize = 10, string? productName = null, int? minRating = null, bool? isApproved = null, CancellationToken ct = default);
    Task<CustomerReviewResponseDto?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<CustomerReviewResponseDto> CreateAsync(CreateCustomerReviewRequest review, CancellationToken ct = default);
    Task UpdateAsync(string id, UpdateCustomerReviewRequest review, CancellationToken ct = default);
    Task ApproveAsync(string id, bool isApproved, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
