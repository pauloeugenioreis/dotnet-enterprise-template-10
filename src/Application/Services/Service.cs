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
        try
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity with ID {Id}", id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _repository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all entities");
            throw;
        }
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _repository.AddAsync(entity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity");
            throw;
        }
    }

    public virtual async Task UpdateAsync(long id, TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _repository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found");
            }

            await _repository.UpdateAsync(entity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity with ID {Id}", id);
            throw;
        }
    }

    public virtual async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found");
            }

            await _repository.DeleteAsync(entity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity with ID {Id}", id);
            throw;
        }
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int Total)> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _repository.GetPagedAsync(page, pageSize, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged entities");
            throw;
        }
    }
}
