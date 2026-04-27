using BlazorApp.Client.Services.Base;
using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services;

public interface IOrderService
{
    Task<PagedResponse<OrderResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? status = null, DateTime? from = null, DateTime? toDate = null, CancellationToken ct = default);
    Task<OrderStatisticsDto> GetStatisticsAsync(CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task CreateAsync(CreateOrderRequest dto, CancellationToken ct = default);
    Task UpdateAsync(long id, UpdateOrderRequest dto, CancellationToken ct = default);
    Task UpdateStatusAsync(long id, UpdateOrderStatusDto dto, CancellationToken ct = default);
}

public class OrderService(IHttpClientFactory httpClientFactory) 
    : BaseService(httpClientFactory, "api/v1/order"), IOrderService
{
    public Task<PagedResponse<OrderResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? status = null, DateTime? from = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        return GetPagedAsync<OrderResponseDto>(new { page, pageSize, searchTerm, status, from, toDate }, ct);
    }

    public Task<OrderStatisticsDto> GetStatisticsAsync(CancellationToken ct = default)
    {
        return GetAsync<OrderStatisticsDto>($"{ResourcePath}/statistics", ct);
    }

    public Task CreateAsync(CreateOrderRequest dto, CancellationToken ct = default)
    {
        return PostAsync<CreateOrderRequest, OrderResponseDto>(ResourcePath!, dto, ct);
    }

    public Task UpdateAsync(long id, UpdateOrderRequest dto, CancellationToken ct = default)
    {
        return PutAsync($"{ResourcePath}/{id}", dto, ct);
    }

    public Task UpdateStatusAsync(long id, UpdateOrderStatusDto dto, CancellationToken ct = default)
    {
        return PatchAsync($"{ResourcePath}/{id}/status", dto, ct);
    }
}
