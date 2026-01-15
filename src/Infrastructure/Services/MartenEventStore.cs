using Marten;
using Marten.Events;
using Marten.Events.Projections;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;
using System.Text.Json;

namespace ProjectTemplate.Infrastructure.Services;

/// <summary>
/// Marten-based Event Store implementation
/// </summary>
public class MartenEventStore : IEventStore
{
    private readonly IDocumentStore _documentStore;
    private readonly EventSourcingSettings _settings;

    public MartenEventStore(IDocumentStore documentStore, EventSourcingSettings settings)
    {
        _documentStore = documentStore;
        _settings = settings;
    }

    public async Task AppendEventAsync<TEvent>(
        string aggregateType,
        string aggregateId,
        TEvent eventData,
        string? userId = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        await using var session = _documentStore.LightweightSession();

        // Create stream ID combining type and ID
        var streamId = $"{aggregateType}-{aggregateId}";

        // Append to Marten event stream
        session.Events.Append(streamId, eventData);

        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var streamId = $"{aggregateType}-{aggregateId}";

        var events = await session.Events.FetchStreamAsync(streamId, token: cancellationToken);

        return events
            .Select(e => ConvertToTypedEvent(e.Data))
            .OfType<DomainEvent>()
            .ToList();
    }

    public async Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        DateTime until,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var streamId = $"{aggregateType}-{aggregateId}";

        var events = await session.Events.FetchStreamAsync(streamId, token: cancellationToken).ConfigureAwait(false);

        return events
            .Where(e => e.Timestamp <= until)
            .Select(e => ConvertToTypedEvent(e.Data))
            .OfType<DomainEvent>()
            .ToList();
    }

    public async Task<List<DomainEvent>> GetEventsByVersionAsync(
        string aggregateType,
        string aggregateId,
        int fromVersion,
        int toVersion,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var streamId = $"{aggregateType}-{aggregateId}";

        var events = await session.Events.FetchStreamAsync(streamId, token: cancellationToken).ConfigureAwait(false);

        return events
            .Where(e => e.Version >= fromVersion && e.Version <= toVersion)
            .Select(e => ConvertToTypedEvent(e.Data))
            .OfType<DomainEvent>()
            .ToList();
    }

    public async Task<List<DomainEvent>> GetEventsByTypeAsync(
        string aggregateType,
        DateTime? from = null,
        DateTime? to = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();

        var query = session.Events.QueryAllRawEvents()
            .Where(e => ((DomainEvent)e.Data).AggregateType == aggregateType)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Timestamp <= to.Value);

        query = query.OrderByDescending(e => e.Timestamp);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        var events = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return events
            .Select(e => ConvertToTypedEvent(e.Data))
            .OfType<DomainEvent>()
            .ToList();
    }

    public async Task<List<DomainEvent>> GetEventsByUserAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();

        var query = session.Events.QueryAllRawEvents()
            .Where(e => ((DomainEvent)e.Data).UserId == userId)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Timestamp <= to.Value);

        query = query.OrderByDescending(e => e.Timestamp);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        var events = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return events
            .Select(e => ConvertToTypedEvent(e.Data))
            .OfType<DomainEvent>()
            .ToList();
    }

    public async Task<int> GetLatestVersionAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var streamId = $"{aggregateType}-{aggregateId}";

        var state = await session.Events.FetchStreamStateAsync(streamId, cancellationToken).ConfigureAwait(false);

        return (int)(state?.Version ?? 0);
    }

    public async Task SaveSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        TSnapshot snapshot,
        int version,
        CancellationToken cancellationToken = default) where TSnapshot : class
    {
        if (!_settings.StoreSnapshots)
            return;

        await using var session = _documentStore.LightweightSession();

        var snapshotDoc = new SnapshotDocument<TSnapshot>
        {
            Id = $"{aggregateType}-{aggregateId}",
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            Snapshot = snapshot,
            Version = version,
            CreatedAt = DateTime.UtcNow
        };

        session.Store(snapshotDoc);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<(TSnapshot? Snapshot, int Version)> GetSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default) where TSnapshot : class
    {
        if (!_settings.StoreSnapshots)
            return (null, 0);

        await using var session = _documentStore.QuerySession();
        var id = $"{aggregateType}-{aggregateId}";

        var snapshot = await session.LoadAsync<SnapshotDocument<TSnapshot>>(id, cancellationToken).ConfigureAwait(false);

        if (snapshot == null)
            return (null, 0);

        return (snapshot.Snapshot, snapshot.Version);
    }

    public async Task<EventStatistics> GetStatisticsAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();

        var query = session.Events.QueryAllRawEvents().AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Timestamp <= to.Value);

        var events = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        var stats = new EventStatistics
        {
            TotalEvents = events.Count
        };

        if (events.Any())
        {
            stats.OldestEvent = events.Min(e => e.Timestamp).DateTime;
            stats.LatestEvent = events.Max(e => e.Timestamp).DateTime;

            // Group by event type
            stats.EventsByType = events
                .Select(e => ConvertToTypedEvent(e.Data))
                .Where(e => e != null)
                .GroupBy(e => e!.EventType)
                .ToDictionary(g => g.Key, g => (long)g.Count());

            // Group by aggregate type
            stats.EventsByAggregateType = events
                .Select(e => ConvertToTypedEvent(e.Data))
                .Where(e => e != null)
                .GroupBy(e => e!.AggregateType)
                .ToDictionary(g => g.Key, g => (long)g.Count());
        }

        return stats;
    }

    private DomainEvent? ConvertToTypedEvent(object eventData)
    {
        if (eventData == null)
            return null;

        // If it's already a DomainEvent, return it
        if (eventData is DomainEvent domainEvent)
            return domainEvent;

        // Otherwise, try to convert it
        try
        {
            var json = JsonSerializer.Serialize(eventData);
            return JsonSerializer.Deserialize<DomainEvent>(json);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Snapshot document for Marten storage
/// </summary>
internal class SnapshotDocument<TSnapshot> where TSnapshot : class
{
    public string Id { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public TSnapshot Snapshot { get; set; } = default!;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
}
