using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.SharedModels;

namespace EnterpriseTemplate.MauiApp.Services;

public interface ICustomerReviewService
{
    Task<PagedResponse<CustomerReviewResponseDto>?> GetReviewsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
}

public class CustomerReviewService : BaseService, ICustomerReviewService
{
    public CustomerReviewService(HttpClient http) : base(http) { }

    public Task<PagedResponse<CustomerReviewResponseDto>?> GetReviewsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => GetAsync<PagedResponse<CustomerReviewResponseDto>>($"api/customer-reviews?page={page}&pageSize={pageSize}", cancellationToken);
}
