using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.SharedModels;

namespace EnterpriseTemplate.MauiApp.Services;

public interface IOrderService
{
    Task<PagedResponse<OrderResponseDto>?> GetOrdersAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<OrderStatisticsDto?> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

public class OrderService : BaseService, IOrderService
{
    public OrderService(HttpClient http) : base(http) { }

    public Task<PagedResponse<OrderResponseDto>?> GetOrdersAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => GetAsync<PagedResponse<OrderResponseDto>>($"api/orders?page={page}&pageSize={pageSize}", cancellationToken);

    public Task<OrderStatisticsDto?> GetStatisticsAsync(CancellationToken cancellationToken = default)
        => GetAsync<OrderStatisticsDto>("api/orders/statistics", cancellationToken);
}
