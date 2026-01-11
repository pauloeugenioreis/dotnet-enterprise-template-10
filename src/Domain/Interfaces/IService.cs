namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Generic service interface for business logic layer
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IService<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<TEntity> Items, int Total)> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
}
