using BlazorApp.Client.Services.Base;
using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services;

public class CustomerReviewService(IHttpClientFactory httpClientFactory) 
    : BaseService(httpClientFactory, "api/v1/CustomerReview"), ICustomerReviewService
{
    public Task<PagedResponse<CustomerReviewResponseDto>> GetReviewsAsync(int page = 1, int pageSize = 10, string? productName = null, int? minRating = null, bool? isApproved = null, CancellationToken ct = default)
    {
        return GetPagedAsync<CustomerReviewResponseDto>(new { page, pageSize, productName, minRating, isApproved }, ct);
    }

    public Task<CustomerReviewResponseDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return GetAsync<CustomerReviewResponseDto>($"{ResourcePath}/{id}", ct);
    }

    public Task<CustomerReviewResponseDto> CreateAsync(CreateCustomerReviewRequest review, CancellationToken ct = default)
    {
        return PostAsync<CreateCustomerReviewRequest, CustomerReviewResponseDto>(ResourcePath!, review, ct);
    }

    public Task UpdateAsync(string id, UpdateCustomerReviewRequest review, CancellationToken ct = default)
    {
        return PutAsync($"{ResourcePath}/{id}", review, ct);
    }

    public Task ApproveAsync(string id, bool isApproved, CancellationToken ct = default)
    {
        return PatchAsync($"{ResourcePath}/{id}/approve", new ApproveCustomerReviewRequest { IsApproved = isApproved }, ct);
    }

    public Task DeleteAsync(string id, CancellationToken ct = default)
    {
        return DeleteRequestAsync($"{ResourcePath}/{id}", ct);
    }
}
