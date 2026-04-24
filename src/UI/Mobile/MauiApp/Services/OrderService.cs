using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.Shared.Models;

namespace EnterpriseTemplate.MauiApp.Services;

public interface IOrderService
{
    Task<PagedResponse<OrderResponseDto>?> GetOrdersAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<OrderResponseDto?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderAsync(int id, object orderData, CancellationToken cancellationToken = default);
    Task<OrderStatisticsDto?> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

public class OrderService : BaseService, IOrderService
{
    public OrderService(HttpClient http) : base(http) { }

    public Task<PagedResponse<OrderResponseDto>?> GetOrdersAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var url = $"api/v1/Order?page={page}&pageSize={pageSize}"; // Aligned with Flutter path
        if (!string.IsNullOrEmpty(searchTerm)) url += $"&searchTerm={searchTerm}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
        if (fromDate.HasValue) url += $"&startDate={fromDate.Value:O}";
        if (toDate.HasValue) url += $"&endDate={toDate.Value:O}";

        return GetAsync<PagedResponse<OrderResponseDto>>(url, cancellationToken);
    }

    public Task<OrderResponseDto?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
        => GetAsync<OrderResponseDto>($"api/v1/Order/{id}", cancellationToken);

    public async Task<bool> UpdateOrderAsync(int id, object orderData, CancellationToken cancellationToken = default)
    {
        await PutAsync<object, object>($"api/v1/Order/{id}", orderData, cancellationToken);
        return true;
    }

    public Task<OrderStatisticsDto?> GetStatisticsAsync(CancellationToken cancellationToken = default)
        => GetAsync<OrderStatisticsDto>("api/orders/statistics", cancellationToken);
}
