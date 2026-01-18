using System.Threading;
using System.Threading.Tasks;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Data.Repository;

/// <summary>
/// Represents a transaction placeholder for providers that do not support real transactions (e.g., EF InMemory).
/// </summary>
internal sealed class NoOpRepositoryTransaction : IRepositoryTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
