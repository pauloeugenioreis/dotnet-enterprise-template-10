using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.SharedModels;

namespace EnterpriseTemplate.MauiApp.Services;

public interface IOrderService
{
    Task<PagedResponse<OrderResponseDto>?> GetOrdersAsync(int page = 1, int pageSize = 10, string searchTerm = null, string status = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<OrderStatisticsDto?> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

public class OrderService : BaseService, IOrderService
{
    public OrderService(HttpClient http) : base(http) { }

    public Task<PagedResponse<OrderResponseDto>?> GetOrdersAsync(int page = 1, int pageSize = 10, string searchTerm = null, string status = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var url = $"api/orders?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm)) url += $"&searchTerm={searchTerm}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
        if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:O}";
        if (toDate.HasValue) url += $"&toDate={toDate.Value:O}";

        return GetAsync<PagedResponse<OrderResponseDto>>(url, cancellationToken);
    }

    public Task<OrderStatisticsDto?> GetStatisticsAsync(CancellationToken cancellationToken = default)
        => GetAsync<OrderStatisticsDto>("api/orders/statistics", cancellationToken);
}
