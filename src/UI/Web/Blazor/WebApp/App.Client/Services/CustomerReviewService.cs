using BlazorApp.Client.Services.Base;
using ProjectTemplate.SharedModels;

namespace BlazorApp.Client.Services;

public interface ICustomerReviewService
{
    Task<PagedResponse<CustomerReviewResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
}

public class CustomerReviewService(IHttpClientFactory httpClientFactory) 
    : BaseService(httpClientFactory), ICustomerReviewService
{
    public Task<PagedResponse<CustomerReviewResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return GetAsync<PagedResponse<CustomerReviewResponseDto>>($"api/v1/customerreview?pageNumber={page}&pageSize={pageSize}", ct);
    }
}
