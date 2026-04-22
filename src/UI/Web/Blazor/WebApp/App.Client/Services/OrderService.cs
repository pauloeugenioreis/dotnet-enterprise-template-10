using BlazorApp.Client.Services.Base;
using ProjectTemplate.SharedModels;

namespace BlazorApp.Client.Services;

public interface IOrderService
{
    Task<PagedResponse<OrderResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<OrderStatisticsDto> GetStatisticsAsync(CancellationToken ct = default);
}

public class OrderService(IHttpClientFactory httpClientFactory) 
    : BaseService(httpClientFactory), IOrderService
{
    public Task<PagedResponse<OrderResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return GetAsync<PagedResponse<OrderResponseDto>>($"api/v1/order?pageNumber={page}&pageSize={pageSize}", ct);
    }

    public Task<OrderStatisticsDto> GetStatisticsAsync(CancellationToken ct = default)
    {
        return GetAsync<OrderStatisticsDto>("api/v1/order/statistics", ct);
    }
}
