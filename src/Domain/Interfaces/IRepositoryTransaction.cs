using System.Threading;
using System.Threading.Tasks;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Represents a database transaction wrapper decoupled from any specific ORM.
/// </summary>
public interface IRepositoryTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides transaction support for repositories that can group multiple operations atomically.
/// </summary>
public interface ITransactionalRepository
{
    Task<IRepositoryTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
