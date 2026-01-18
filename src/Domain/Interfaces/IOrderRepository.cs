using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Order-specific repository interface with custom methods
/// </summary>
public interface IOrderRepository : IRepository<Order>, ITransactionalRepository
{
    /// <summary>
    /// Get orders by customer email
    /// </summary>
    Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders by status
    /// </summary>
    Task<IEnumerable<Order>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders within date range
    /// </summary>
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order with items by order number
    /// </summary>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
}
