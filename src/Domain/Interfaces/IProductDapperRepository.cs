using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Dapper-specific interface for Product repository.
/// Inherits from IRepository of Product but uses specific interface to avoid Scrutor auto-registration.
/// </summary>
public interface IProductDapperRepository : IRepository<Product>
{
}
