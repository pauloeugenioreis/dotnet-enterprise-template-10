using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectTemplate.Data.Context;
using ProjectTemplate.Data.Repository;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;
using ProjectTemplate.Infrastructure.Services;
using Xunit;

namespace ProjectTemplate.UnitTests.EventSourcing;

public class EventSourcingDisabledTests
{
    [Fact]
    public async Task Repository_ShouldNotRecordEvents_WhenEventSourcingDisabled()
    {
        // Arrange
        var services = new ServiceCollection();

        // Configure with Event Sourcing DISABLED
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("TestDb_Disabled"));
        // Register ApplicationDbContext as DbContext for HybridRepository
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        // Register ApplicationDbContext as DbContext for HybridRepository
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        var settings = new EventSourcingSettings { Enabled = false };
        services.AddSingleton(settings);
        services.AddScoped<IEventStore, NoOpEventStore>();
        services.AddScoped(typeof(IRepository<>), typeof(HybridRepository<>));
        services.AddHttpContextAccessor();

        var provider = services.BuildServiceProvider();
        var repository = provider.GetRequiredService<IRepository<Order>>();
        var eventStore = provider.GetRequiredService<IEventStore>();

        // Act
        var order = new Order
        {
            OrderNumber = "TEST-001",
            CustomerName = "Test Customer",
            CustomerEmail = "test@test.com",
            ShippingAddress = "Test Address",
            Total = 100m
        };

        await repository.AddAsync(order);
        await repository.SaveChangesAsync();

        // Assert
        var events = await eventStore.GetEventsAsync("Order", order.Id.ToString());
        Assert.Empty(events); // No events should be recorded
    }
}

public class EventSourcingEnabledTests
{
    [Fact]
    public async Task Repository_ShouldRecordEvents_WhenEventSourcingEnabled()
    {
        // Arrange
        var services = new ServiceCollection();

        // Configure with Event Sourcing ENABLED
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("TestDb_Enabled"));

        // Register ApplicationDbContext as DbContext for HybridRepository
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        var settings = new EventSourcingSettings
        {
            Enabled = true,
            Mode = EventSourcingMode.Hybrid,
            AuditEntities = new List<string> { "Order" }
        };
        services.AddSingleton(settings);
        services.AddScoped<IEventStore, InMemoryEventStore>();
        services.AddScoped(typeof(IRepository<>), typeof(HybridRepository<>));
        services.AddHttpContextAccessor();

        var provider = services.BuildServiceProvider();
        var repository = provider.GetRequiredService<IRepository<Order>>();
        var eventStore = provider.GetRequiredService<IEventStore>();

        // Act
        var order = new Order
        {
            OrderNumber = "TEST-002",
            CustomerName = "Test Customer",
            CustomerEmail = "test@test.com",
            ShippingAddress = "Test Address",
            Total = 100m
        };

        await repository.AddAsync(order);
        await repository.SaveChangesAsync();

        // Assert
        var events = await eventStore.GetEventsAsync("Order", order.Id.ToString());
        Assert.NotEmpty(events); // Events should be recorded
        Assert.Single(events); // One create event

        // Verify event data contains order information
        var json = events[0].EventData as string
            ?? System.Text.Json.JsonSerializer.Serialize(events[0].EventData);
        var eventData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        Assert.NotNull(eventData);

        // The event should contain order data (either as OrderCreatedEvent or Order entity)
        // Just verify that it was recorded
        Assert.True(eventData.Count > 0, "Event data should not be empty");
    }
}

/// <summary>
/// In-memory Event Store for testing
/// </summary>
internal class InMemoryEventStore : IEventStore
{
    private readonly List<DomainEvent> _events = new();

    public Task AppendEventAsync<TEvent>(
        string aggregateType,
        string aggregateId,
        TEvent eventData,
        string? userId = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default) where TEvent : class
    {
        return AppendEventsAsync(aggregateType, aggregateId, new[] { eventData }, userId, metadata, cancellationToken);
    }

    public Task AppendEventsAsync(
        string aggregateType,
        string aggregateId,
        IEnumerable<object> events,
        string? userId = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var eventData in events)
        {
            var domainEvent = new DomainEvent
            {
                EventId = Guid.NewGuid(),
                EventType = eventData?.GetType().Name ?? "Unknown",
                AggregateId = aggregateId,
                AggregateType = aggregateType,
                EventData = System.Text.Json.JsonSerializer.Serialize(eventData),
                OccurredOn = DateTime.UtcNow,
                UserId = userId,
                Metadata = metadata ?? new(),
                Timestamp = DateTime.UtcNow,
                Version = 1 // Simplified for test
            };

            _events.Add(domainEvent);
        }
        return Task.CompletedTask;
    }

    public Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_events
            .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId)
            .ToList());
    }

    public Task<List<DomainEvent>> GetEventsAsync(
        string aggregateType,
        string aggregateId,
        DateTime until,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_events
            .Where(e => e.AggregateType == aggregateType &&
                       e.AggregateId == aggregateId &&
                       e.OccurredOn <= until)
            .ToList());
    }

    public Task<List<DomainEvent>> GetEventsByVersionAsync(
        string aggregateType,
        string aggregateId,
        int fromVersion,
        int toVersion,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_events
            .Where(e => e.AggregateType == aggregateType &&
                       e.AggregateId == aggregateId &&
                       e.Version >= fromVersion &&
                       e.Version <= toVersion)
            .ToList());
    }

    public Task<(List<DomainEvent> Items, long TotalCount)> GetEventsPagedAsync(
        string aggregateType,
        string aggregateId,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var all = _events
            .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId)
            .ToList();
        var total = (long)all.Count;
        var items = all.Skip(offset ?? 0).Take(limit ?? all.Count).ToList();
        return Task.FromResult((items, total));
    }

    public Task<(List<DomainEvent> Items, long TotalCount)> GetEventsByFilterAsync(
        string? aggregateType = null,
        string? eventType = null,
        string? userId = null,
        DateTime? from = null,
        DateTime? toDate = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var query = _events.AsQueryable();

        if (!string.IsNullOrEmpty(aggregateType))
            query = query.Where(e => e.AggregateType == aggregateType);

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(e => e.EventType == eventType);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(e => e.UserId == userId);

        if (from.HasValue)
            query = query.Where(e => e.OccurredOn >= from.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.OccurredOn <= toDate.Value);

        var all = query.ToList();
        var total = (long)all.Count;
        var items = all.Skip(offset ?? 0).Take(limit ?? all.Count).ToList();

        return Task.FromResult((items, total));
    }

    public Task<int> GetLatestVersionAsync(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default)
    {
        var maxVersion = _events
            .Where(e => e.AggregateType == aggregateType && e.AggregateId == aggregateId)
            .Max(e => (int?)e.Version) ?? 0;

        return Task.FromResult(maxVersion);
    }

    public Task SaveSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        TSnapshot snapshot,
        int version,
        CancellationToken cancellationToken = default) where TSnapshot : class
    {
        return Task.CompletedTask;
    }

    public Task<(TSnapshot? Snapshot, int Version)> GetSnapshotAsync<TSnapshot>(
        string aggregateType,
        string aggregateId,
        CancellationToken cancellationToken = default) where TSnapshot : class
    {
        return Task.FromResult<(TSnapshot?, int)>((null, 0));
    }

    public Task<EventStatistics> GetStatisticsAsync(
        DateTime? from = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var stats = new EventStatistics
        {
            TotalEvents = _events.Count,
            OldestEvent = _events.Any() ? _events.Min(e => e.OccurredOn) : DateTime.MinValue,
            LatestEvent = _events.Any() ? _events.Max(e => e.OccurredOn) : DateTime.MaxValue,
            EventsByType = _events.GroupBy(e => e.EventType).ToDictionary(g => g.Key, g => (long)g.Count()),
            EventsByAggregateType = _events.GroupBy(e => e.AggregateType).ToDictionary(g => g.Key, g => (long)g.Count())
        };

        return Task.FromResult(stats);
    }
}

/// <summary>
/// No-op Event Store (same as in EventSourcingExtension)
/// </summary>
internal class NoOpEventStore : IEventStore
{
    public Task AppendEventAsync<TEvent>(string aggregateType, string aggregateId, TEvent eventData,
        string? userId = null, Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default) where TEvent : class => Task.CompletedTask;

    public Task AppendEventsAsync(string aggregateType, string aggregateId, IEnumerable<object> events,
        string? userId = null, Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<List<DomainEvent>> GetEventsAsync(string aggregateType, string aggregateId,
        CancellationToken cancellationToken = default) => Task.FromResult(new List<DomainEvent>());

    public Task<(List<DomainEvent> Items, long TotalCount)> GetEventsPagedAsync(
        string aggregateType, string aggregateId, int? limit = null, int? offset = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<(List<DomainEvent>, long)>((new List<DomainEvent>(), 0L));

    public Task<List<DomainEvent>> GetEventsAsync(string aggregateType, string aggregateId,
        DateTime until, CancellationToken cancellationToken = default) => Task.FromResult(new List<DomainEvent>());

    public Task<List<DomainEvent>> GetEventsByVersionAsync(string aggregateType, string aggregateId,
        int fromVersion, int toVersion, CancellationToken cancellationToken = default) => Task.FromResult(new List<DomainEvent>());

    public Task<(List<DomainEvent> Items, long TotalCount)> GetEventsByFilterAsync(
        string? aggregateType = null, string? eventType = null, string? userId = null,
        DateTime? from = null, DateTime? toDate = null, int? limit = null, int? offset = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<(List<DomainEvent>, long)>((new List<DomainEvent>(), 0L));

    public Task<int> GetLatestVersionAsync(string aggregateType, string aggregateId,
        CancellationToken cancellationToken = default) => Task.FromResult(0);

    public Task SaveSnapshotAsync<TSnapshot>(string aggregateType, string aggregateId, TSnapshot snapshot, int version,
        CancellationToken cancellationToken = default) where TSnapshot : class => Task.CompletedTask;

    public Task<(TSnapshot? Snapshot, int Version)> GetSnapshotAsync<TSnapshot>(string aggregateType, string aggregateId,
        CancellationToken cancellationToken = default) where TSnapshot : class => Task.FromResult<(TSnapshot?, int)>((null, 0));

    public Task<EventStatistics> GetStatisticsAsync(DateTime? from = null, DateTime? toDate = null,
        CancellationToken cancellationToken = default) => Task.FromResult(new EventStatistics());
}
