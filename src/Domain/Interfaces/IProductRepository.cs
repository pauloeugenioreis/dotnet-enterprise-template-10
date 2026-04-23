using ProjectTemplate.SharedModels;
using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Product-specific repository interface
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<(IEnumerable<Product> Items, int Total)> GetByFilterAsync(
        string? searchTerm = null,
        bool? isActive = null, 
        int? page = null, 
        int? pageSize = null, 
        CancellationToken cancellationToken = default);
}
