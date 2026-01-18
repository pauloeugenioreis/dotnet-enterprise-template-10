using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Data.Repository;

/// <summary>
/// Generic Repository implementation using Entity Framework Core
/// This is the default ORM implementation
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public class Repository<TEntity> : IRepository<TEntity>, ITransactionalRepository where TEntity : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        await _dbSet.AddRangeAsync(entityList, cancellationToken);
        return entityList;
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        // Get the ID value from the incoming entity
        var entityEntry = _context.Entry(entity);
        var idValue = entityEntry.Property("Id").CurrentValue;

        // Find the existing tracked entity with the same ID
        var existingEntity = await _dbSet.FindAsync(new object[] { idValue! }, cancellationToken);

        if (existingEntity != null)
        {
            // Copy values from the incoming entity to the tracked entity
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        }
        else
        {
            // If entity doesn't exist in the database, attach and mark as modified
            _dbSet.Attach(entity);
            entityEntry.State = EntityState.Modified;
        }
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int Total)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var total = await _dbSet.CountAsync(cancellationToken);
        var items = await _dbSet
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<IRepositoryTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (!_context.Database.IsRelational())
        {
            return new NoOpRepositoryTransaction();
        }

        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new EfRepositoryTransaction(transaction);
    }
}
