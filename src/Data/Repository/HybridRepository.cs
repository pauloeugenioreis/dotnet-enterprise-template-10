using System.Collections.Generic;
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
    private readonly IEventPayloadFactory<TEntity>? _payloadFactory;
    private readonly IExecutionContextService? _executionContextService;
    private readonly List<Func<CancellationToken, Task>> _pendingEventDispatchers = new();

    public HybridRepository(
        DbContext context,
        IEventStore eventStore,
        EventSourcingSettings settings,
        IExecutionContextService? executionContextService = null,
        IEventPayloadFactory<TEntity>? payloadFactory = null)
        : base(context)
    {
        _eventStore = eventStore;
        _settings = settings;
        _executionContextService = executionContextService;
        _payloadFactory = payloadFactory;
    }

    public override async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // 1. Save to EF Core (traditional approach)
        var result = await base.AddAsync(entity, cancellationToken);

        // 2. Record event if Event Sourcing is enabled and entity is in audit list
        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            EnqueueEvent(ct => RecordCreatedEvent(entity, ct));
        }

        return result;
    }

    public override async Task<IEnumerable<TEntity>> AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        var result = await base.AddRangeAsync(entityList, cancellationToken);

        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            foreach (var entity in entityList)
            {
                var capturedEntity = entity;
                EnqueueEvent(ct => RecordCreatedEvent(capturedEntity, ct));
            }
        }

        return result;
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // 1. Update in EF Core (this will attach and track changes)
        await base.UpdateAsync(entity, cancellationToken);

        // 2. Detect changes from ChangeTracker
        Dictionary<string, object>? changes = null;
        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            changes = DetectChanges(entity);
        }

        // 3. Record event
        if (changes != null && changes.Count > 0)
        {
            var changesSnapshot = new Dictionary<string, object>(changes);
            EnqueueEvent(ct => RecordUpdatedEvent(entity, changesSnapshot, ct));
        }

        return;
    }

    public override async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // 1. Delete from EF Core
        await base.DeleteAsync(entity, cancellationToken);

        // 2. Record event
        if (ShouldAuditEntity(typeof(TEntity).Name))
        {
            EnqueueEvent(ct => RecordDeletedEvent(entity, ct));
        }

        return;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        if (_pendingEventDispatchers.Count == 0)
        {
            return result;
        }

        try
        {
            foreach (var dispatcher in _pendingEventDispatchers)
            {
                await dispatcher(cancellationToken);
            }
        }
        finally
        {
            _pendingEventDispatchers.Clear();
        }

        return result;
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

    private void EnqueueEvent(Func<CancellationToken, Task> dispatcher)
    {
        _pendingEventDispatchers.Add(dispatcher);
    }

    private Task RecordCreatedEvent(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

        object payload = _payloadFactory != null 
            ? _payloadFactory.CreateCreatedEvent(entity) 
            : entity;

        return AppendDomainEventAsync(entityType, entityId, payload, cancellationToken);
    }

    private Task RecordUpdatedEvent(
        TEntity entity,
        Dictionary<string, object> changes,
        CancellationToken cancellationToken)
    {
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

        object payload = _payloadFactory != null
            ? _payloadFactory.CreateUpdatedEvent(entity, changes)
            : new { EntityId = entity.Id, Changes = changes };

        return AppendDomainEventAsync(entityType, entityId, payload, cancellationToken);
    }

    private Task RecordDeletedEvent(TEntity entity, CancellationToken cancellationToken)
    {
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);

        object payload = _payloadFactory != null
            ? _payloadFactory.CreateDeletedEvent(entity)
            : new { EntityId = entity.Id };

        return AppendDomainEventAsync(entityType, entityId, payload, cancellationToken);
    }

    private Task AppendDomainEventAsync<TEvent>(
        string aggregateType,
        string aggregateId,
        TEvent payload,
        CancellationToken cancellationToken) where TEvent : class
    {
        var metadata = GetMetadata();

        return _eventStore.AppendEventAsync(
            aggregateType: aggregateType,
            aggregateId: aggregateId,
            eventData: payload,
            userId: GetCurrentUserId(),
            metadata: metadata,
            cancellationToken: cancellationToken);
    }

    private Dictionary<string, object> DetectChanges(TEntity entity)
    {
        var changes = new Dictionary<string, object>();
        var idValue = Context.Entry(entity).Property(nameof(EntityBase.Id)).CurrentValue;

        var trackedEntry = Context.ChangeTracker.Entries<TEntity>()
            .FirstOrDefault(e => Equals(e.Property(nameof(EntityBase.Id)).CurrentValue, idValue));

        if (trackedEntry == null)
        {
            return changes;
        }

        foreach (var property in trackedEntry.Properties.Where(p => p.IsModified))
        {
            var currentValue = property.CurrentValue;
            var originalValue = property.OriginalValue;

            if (!Equals(currentValue, originalValue))
            {
                changes[property.Metadata.Name] = new
                {
                    Old = originalValue,
                    New = currentValue
                };
            }
        }

        // If no changes were detected because it was attached as Modified,
        // we can at least return a marker indicating an update occurred.
        if (changes.Count == 0 && trackedEntry.State == EntityState.Modified)
        {
            changes["_State"] = new { Status = "Updated (No explicit delta)" };
        }

        return changes;
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
