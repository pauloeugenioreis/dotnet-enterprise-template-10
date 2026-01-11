using ProjectTemplate.Domain.Dtos;
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
    /// Calculate order totals
    /// </summary>
    Task<decimal> CalculateOrderTotalAsync(long orderId, CancellationToken cancellationToken = default);
}
