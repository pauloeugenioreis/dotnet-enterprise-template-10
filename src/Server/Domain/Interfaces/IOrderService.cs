using ProjectTemplate.Shared.Models;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Order service interface with business logic
/// </summary>
public interface IOrderService : IService<Order>
{
    /// <summary>
    /// Create order from DTO
    /// </summary>
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequest dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update order status
    /// </summary>
    Task<OrderResponseDto> UpdateOrderStatusAsync(long id, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order details with calculated totals
    /// </summary>
    Task<OrderResponseDto?> GetOrderDetailsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by customer
    /// </summary>
    Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by status
    /// </summary>
    Task<IEnumerable<OrderResponseDto>> GetOrdersByStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all orders with details, with optional pagination.
    /// When page/pageSize are null, returns all records.
    /// </summary>
    Task<(IEnumerable<OrderResponseDto> Items, int Total)> GetAllOrderDetailsAsync(
        string? status = null, 
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? page = null, 
        int? pageSize = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate order totals
    /// </summary>
    Task<decimal> CalculateOrderTotalAsync(long orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get aggregated order statistics computed at the database level.
    /// </summary>
    Task<OrderStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
