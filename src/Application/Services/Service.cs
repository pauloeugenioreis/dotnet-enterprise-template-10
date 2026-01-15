using ProjectTemplate.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ProjectTemplate.Application.Services;

/// <summary>
/// Generic Service base class implementing common business logic
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public class Service<TEntity> : IService<TEntity> where TEntity : class
{
    protected readonly IRepository<TEntity> _repository;
    protected readonly ILogger<Service<TEntity>> _logger;

    public Service(IRepository<TEntity> repository, ILogger<Service<TEntity>> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public virtual async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var created = await _repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return created;
    }

    public virtual async Task UpdateAsync(long id, TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentNullException.ThrowIfNull(entity);

        var existing = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Entity with ID {id} not found");
        }

        await _repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

        var entity = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Entity with ID {id} not found");
        }

        await _repository.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        return await _repository.GetPagedAsync(page, pageSize, cancellationToken).ConfigureAwait(false);
    }
}
