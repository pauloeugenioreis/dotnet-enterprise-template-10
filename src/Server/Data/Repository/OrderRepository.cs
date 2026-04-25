using ProjectTemplate.Shared.Models;
using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Data.Context;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Data.Repository;

/// <summary>
/// Order repository with custom query methods
/// </summary>
public class OrderRepository : HybridRepository<Order>, IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(
        ApplicationDbContext context,
        IEventStore eventStore,
        EventSourcingSettings settings,
        IExecutionContextService? executionContextService = null,
        IEventPayloadFactory<Order>? payloadFactory = null)
        : base(context, eventStore, settings, executionContextService, payloadFactory)
    {
        _dbContext = context;
    }

    public async Task<(IEnumerable<Order> Items, int Total)> GetByFilterAsync(
        string? status = null, 
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? page = null, 
        int? pageSize = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        if (startDate.HasValue)
            query = query.Where(o => o.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.CreatedAt <= endDate.Value);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLower();
            
            // Try to match by ID if numeric
            bool isIdSearch = long.TryParse(searchTerm, out var idSearch);

            query = query.Where(o => 
                (isIdSearch && o.Id == idSearch) ||
                o.OrderNumber.ToLower().Contains(term) ||
                o.CustomerName.ToLower().Contains(term) ||
                o.CustomerEmail.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        query = query
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.UpdatedAt);

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        var items = await query.ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.CustomerEmail == email)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public override async Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders.AsNoTracking();

        var totalOrders = await query.CountAsync(cancellationToken);

        if (totalOrders == 0)
        {
            return new OrderStatisticsDto();
        }

        var totalRevenue = await query.SumAsync(o => o.Total, cancellationToken);
        var avgOrderValue = await query.AverageAsync(o => o.Total, cancellationToken);

        // Fetch lightweight projections and aggregate client-side
        // (InMemory provider doesn't support GroupBy with constructor projections)
        var orderStatusData = await query
            .Select(o => new { o.Status, o.Total })
            .ToListAsync(cancellationToken);

        var ordersByStatus = orderStatusData
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusStatDto(g.Key, g.Count(), g.Sum(o => o.Total)))
            .ToList();

        var orderItemData = await _dbContext.Set<OrderItem>()
            .AsNoTracking()
            .Select(i => new { i.ProductId, i.ProductName, i.Quantity, i.Subtotal })
            .ToListAsync(cancellationToken);

        var topProducts = orderItemData
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductStatDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Sum(i => i.Quantity),
                g.Sum(i => i.Subtotal)))
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToList();

        return new OrderStatisticsDto
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            AverageOrderValue = avgOrderValue,
            OrdersByStatus = ordersByStatus,
            TopProducts = topProducts
        };
    }
}
