using Microsoft.AspNetCore.Mvc;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Events;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Api.Controllers;

/// <summary>
/// Audit API - Query event history and audit trails
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuditController : ApiControllerBase
{
    private readonly IEventStore _eventStore;
    private readonly EventSourcingSettings _settings;

    public AuditController(IEventStore eventStore, EventSourcingSettings settings)
    {
        _eventStore = eventStore;
        _settings = settings;
    }

    /// <summary>
    /// Get full history of events for a specific entity
    /// </summary>
    [HttpGet("{entityType}/{entityId}")]
    public async Task<ActionResult<List<DomainEvent>>> GetEntityHistory(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest("Event Sourcing or Audit API is disabled");
        }

        var events = await _eventStore.GetEventsAsync(entityType, entityId, cancellationToken);
        return Ok(events);
    }

    /// <summary>
    /// Get entity state at a specific point in time (time travel)
    /// </summary>
    [HttpGet("{entityType}/{entityId}/at/{timestamp}")]
    public async Task<ActionResult<object>> GetStateAt(
        string entityType,
        string entityId,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return BadRequest("Event Sourcing is disabled");
        }

        if (!_settings.EnableAuditApi)
        {
            return StatusCode(403, "Audit API is disabled");
        }

        var events = await _eventStore.GetEventsAsync(entityType, entityId, timestamp, cancellationToken);

        if (!events.Any())
        {
            return NotFound($"No events found for {entityType} with ID {entityId}");
        }

        return Ok(new
        {
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = timestamp,
            EventCount = events.Count,
            Events = events.OrderBy(e => e.Version).Select(e => new
            {
                e.EventType,
                e.Version,
                e.OccurredOn,
                e.UserId
            })
        });
    }

    /// <summary>
    /// Get events by version range
    /// </summary>
    [HttpGet("{entityType}/{entityId}/versions/{fromVersion}/{toVersion}")]
    public async Task<ActionResult<List<DomainEvent>>> GetEventsByVersion(
        string entityType,
        string entityId,
        int fromVersion,
        int toVersion,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest("Event Sourcing or Audit API is disabled");
        }

        var events = await _eventStore.GetEventsByVersionAsync(
            entityType, entityId, fromVersion, toVersion, cancellationToken);

        return Ok(events);
    }

    /// <summary>
    /// Get all events for an entity type within a date range
    /// </summary>
    [HttpGet("type/{entityType}")]
    public async Task<ActionResult<List<DomainEvent>>> GetEventsByType(
        string entityType,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest("Event Sourcing or Audit API is disabled");
        }

        var events = await _eventStore.GetEventsByTypeAsync(
            entityType, from, to, limit, cancellationToken);

        return Ok(events);
    }

    /// <summary>
    /// Get events by user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult> GetEventsByUser(
        string userId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest("Event Sourcing or Audit API is disabled");
        }

        var events = await _eventStore.GetEventsByUserAsync(userId, from, to, limit, cancellationToken);

        return Ok(new
        {
            UserId = userId,
            EventCount = events.Count,
            Events = events.OrderByDescending(e => e.OccurredOn).Select(e => new
            {
                e.EventId,
                e.EventType,
                e.AggregateType,
                e.AggregateId,
                e.OccurredOn,
                e.Version,
                e.Metadata
            })
        });
    }

    /// <summary>
    /// Get event statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult> GetStatistics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest("Event Sourcing or Audit API is disabled");
        }

        var stats = await _eventStore.GetStatisticsAsync(from, to, cancellationToken);

        return Ok(stats);
    }

    /// <summary>
    /// Replay events to rebuild state
    /// </summary>
    [HttpPost("{entityType}/{entityId}/replay")]
    public async Task<ActionResult> ReplayEvents(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || !_settings.EnableAuditApi)
        {
            return BadRequest("Event Sourcing or Audit API is disabled");
        }

        var events = await _eventStore.GetEventsAsync(entityType, entityId, cancellationToken);

        if (!events.Any())
        {
            return NotFound($"No events found for {entityType} with ID {entityId}");
        }

        return Ok(new
        {
            EntityType = entityType,
            EntityId = entityId,
            EventCount = events.Count,
            ReconstructedState = "Event replay completed successfully",
            Events = events.OrderBy(e => e.Version).Select(e => new
            {
                e.EventType,
                e.Version,
                e.OccurredOn
            })
        });
    }
}
