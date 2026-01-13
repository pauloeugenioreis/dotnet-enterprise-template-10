using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// ADO.NET-specific interface for Product repository
/// Inherits from IRepository<Product> but uses specific interface to avoid Scrutor auto-registration
/// </summary>
public interface IProductAdoRepository : IRepository<Product>
{
}
