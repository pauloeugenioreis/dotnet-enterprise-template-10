using System.Collections.Generic;
using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Optional factory to create typed events for a specific entity type.
/// If not registered, the event sourcing infrastructure will fall back to
/// serializing the entity or a generic object directly.
/// </summary>
/// <typeparam name="TEntity">The entity type this factory produces events for.</typeparam>
public interface IEventPayloadFactory<TEntity> where TEntity : EntityBase
{
    /// <summary>
    /// Creates the payload for when the entity is created.
    /// </summary>
    object CreateCreatedEvent(TEntity entity);

    /// <summary>
    /// Creates the payload for when the entity is updated.
    /// </summary>
    object CreateUpdatedEvent(TEntity entity, Dictionary<string, object> changes);

    /// <summary>
    /// Creates the payload for when the entity is deleted.
    /// </summary>
    object CreateDeletedEvent(TEntity entity);
}
