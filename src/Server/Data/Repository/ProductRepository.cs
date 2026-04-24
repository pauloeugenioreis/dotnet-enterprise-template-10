using ProjectTemplate.Shared.Models;
using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Data.Context;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Data.Repository;

/// <summary>
/// Product repository with EF Core and event sourcing support
/// </summary>
public class ProductRepository : HybridRepository<Product>, IProductRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductRepository(
        ApplicationDbContext context,
        IEventStore eventStore,
        EventSourcingSettings settings,
        IExecutionContextService? executionContextService = null,
        IEventPayloadFactory<Product>? payloadFactory = null)
        : base(context, eventStore, settings, executionContextService, payloadFactory)
    {
        _dbContext = context;
    }

    public async Task<(IEnumerable<Product> Items, int Total)> GetByFilterAsync(
        string? searchTerm = null,
        bool? isActive = null, 
        int? page = null, 
        int? pageSize = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(term) || 
                (p.Category != null && p.Category.ToLower().Contains(term)) ||
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .OrderBy(p => p.Id)
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        var items = await query.ToListAsync(cancellationToken);
        return (items, total);
    }
}
