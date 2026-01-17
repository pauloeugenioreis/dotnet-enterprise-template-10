using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Events;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Data.Repository;

/// <summary>
/// Hybrid Repository - Saves to EF Core and optionally records events for audit trail
/// </summary>
public class HybridRepository<TEntity> : Repository<TEntity> where TEntity : EntityBase
{
    private readonly IEventStore _eventStore;
    private readonly EventSourcingSettings _settings;
    private readonly IExecutionContextService? _executionContextService;

    public HybridRepository(
        DbContext context,
        IEventStore eventStore,
        EventSourcingSettings settings,
        IExecutionContextService? executionContextService = null)
        : base(context)
    {
        _eventStore = eventStore;
        _settings = settings;
        _executionContextService = executionContextService;
    }

    public override async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // 1. Save to EF Core (traditional approach)
        var result = await base.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Record event if Event Sourcing is enabled and entity is in audit list
        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            await RecordCreatedEvent(entity, cancellationToken);
        }

        return result;
    }

    public override async Task<IEnumerable<TEntity>> AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        var result = await base.AddRangeAsync(entities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            foreach (var entity in entities)
            {
                await RecordCreatedEvent(entity, cancellationToken);
            }
        }

        return result;
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Detect changes before update
        Dictionary<string, object>? changes = null;
        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            changes = await DetectChangesAsync(entity, cancellationToken);
        }

        // 1. Update in EF Core
        await base.UpdateAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Record event
        if (changes != null && changes.Count > 0)
        {
            await RecordUpdatedEvent(entity, changes, cancellationToken);
        }

        return;
    }

    public override async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // 1. Delete from EF Core
        await base.DeleteAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Record event
        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            await RecordDeletedEvent(entity, cancellationToken);
        }

        return;
    }

    private bool ShouldAuditEntity(string entityType)
    {
        if (!_settings.Enabled)
        {
            return false;
        }

        // If AuditEntities is empty, audit all entities
        if (_settings.AuditEntities.Count == 0)
        {
            return true;
        }

        // Otherwise, check if entity is in the list
        return _settings.AuditEntities.Contains(entityType);
    }

    private async Task RecordCreatedEvent(TEntity entity, CancellationToken cancellationToken)
    {
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id.ToString();

        // Create type-specific event based on entity type
        object eventData = entityType switch
        {
            "Order" => CreateOrderCreatedEvent(entity as Order),
            "Product" => CreateProductCreatedEvent(entity as Product),
            _ => entity // Generic fallback
        };

        var metadata = GetMetadata();

        await _eventStore.AppendEventAsync(
            aggregateType: entityType,
            aggregateId: entityId,
            eventData: eventData,
            userId: GetCurrentUserId(),
            metadata: metadata,
            cancellationToken: cancellationToken);
    }

    private async Task RecordUpdatedEvent(
        TEntity entity,
        Dictionary<string, object> changes,
        CancellationToken cancellationToken)
    {
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id.ToString();

        // Create type-specific update event
        object eventData = entityType switch
        {
            "Order" => new OrderUpdatedEvent
            {
                OrderId = entity.Id,
                Changes = changes
            },
            "Product" => new ProductUpdatedEvent
            {
                ProductId = entity.Id,
                Changes = changes
            },
            _ => new { EntityId = entity.Id, Changes = changes }
        };

        var metadata = GetMetadata();

        await _eventStore.AppendEventAsync(
            aggregateType: entityType,
            aggregateId: entityId,
            eventData: eventData,
            userId: GetCurrentUserId(),
            metadata: metadata,
            cancellationToken: cancellationToken);
    }

    private async Task RecordDeletedEvent(TEntity entity, CancellationToken cancellationToken)
    {
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id.ToString();

        object eventData = entityType switch
        {
            "Order" => new OrderDeletedEvent { OrderId = entity.Id },
            "Product" => new ProductDeletedEvent { ProductId = entity.Id },
            _ => new { EntityId = entity.Id }
        };

        var metadata = GetMetadata();

        await _eventStore.AppendEventAsync(
            aggregateType: entityType,
            aggregateId: entityId,
            eventData: eventData,
            userId: GetCurrentUserId(),
            metadata: metadata,
            cancellationToken: cancellationToken);
    }

    private async Task<Dictionary<string, object>> DetectChangesAsync(
        TEntity entity,
        CancellationToken cancellationToken)
    {
        var changes = new Dictionary<string, object>();

        var entry = _context.Entry(entity);
        var existingEntity = await _dbSet.FindAsync(new object[] { entity.Id }, cancellationToken);

        if (existingEntity == null)
        {
            return changes;
        }

        var existingEntry = _context.Entry(existingEntity);

        foreach (var property in entry.Properties)
        {
            var currentValue = property.CurrentValue;
            var originalValue = existingEntry.Property(property.Metadata.Name).CurrentValue;

            if (!Equals(currentValue, originalValue))
            {
                changes[property.Metadata.Name] = new
                {
                    Old = originalValue,
                    New = currentValue
                };
            }
        }

        return changes;
    }

    private OrderCreatedEvent CreateOrderCreatedEvent(Order? order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }

        return new OrderCreatedEvent
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            ShippingAddress = order.ShippingAddress,
            Subtotal = order.Subtotal,
            Tax = order.Tax,
            ShippingCost = order.ShippingCost,
            Total = order.Total,
            Notes = order.Notes,
            Items = order.Items?.Select(i => new OrderItemData
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList() ?? new List<OrderItemData>()
        };
    }

    private ProductCreatedEvent CreateProductCreatedEvent(Product? product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        return new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,  // Changed from StockQuantity
            Category = product.Category,
            IsActive = product.IsActive
        };
    }

    private string? GetCurrentUserId()
    {
        return _executionContextService?.GetCurrentUserId() ?? "system";
    }

    private Dictionary<string, string> GetMetadata()
    {
        if (!_settings.StoreMetadata)
        {
            return new Dictionary<string, string>();
        }

        return _executionContextService?.GetMetadata() ?? new Dictionary<string, string>
        {
            ["Timestamp"] = DateTime.UtcNow.ToString("O"),
            ["MachineName"] = Environment.MachineName
        };
    }
}
