using BlazorApp.Client.Services.Base;
using ProjectTemplate.SharedModels;

namespace BlazorApp.Client.Services;

public interface IOrderService
{
    Task<PagedResponse<OrderResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? search = null, string? status = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    Task<OrderStatisticsDto> GetStatisticsAsync(CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task CreateAsync(CreateOrderRequest dto, CancellationToken ct = default);
    Task UpdateAsync(long id, UpdateOrderRequest dto, CancellationToken ct = default);
    Task UpdateStatusAsync(long id, UpdateOrderStatusDto dto, CancellationToken ct = default);
}

public class OrderService(IHttpClientFactory httpClientFactory, LocalStorageService localStorage) 
    : BaseService(httpClientFactory, localStorage, "api/v1/order"), IOrderService
{
    public Task<PagedResponse<OrderResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? search = null, string? status = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var url = $"{ResourcePath}?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) url += $"&searchTerm={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
        if (from.HasValue) url += $"&from={from.Value:yyyy-MM-dd}";
        if (to.HasValue) url += $"&toDate={to.Value:yyyy-MM-dd}";
        return GetAsync<PagedResponse<OrderResponseDto>>(url, ct);
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
