using BlazorApp.Client.Services.Base;
using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services;

public interface ICustomerReviewService
{
    Task<PagedResponse<CustomerReviewResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
}

public class CustomerReviewService(IHttpClientFactory httpClientFactory, LocalStorageService localStorage) 
    : BaseService(httpClientFactory, localStorage, "api/v1/customerreview"), ICustomerReviewService
{
    public Task<PagedResponse<CustomerReviewResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return GetAsync<PagedResponse<CustomerReviewResponseDto>>($"{ResourcePath}?pageNumber={page}&pageSize={pageSize}", ct);
    }
}
