using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Dapper-specific interface for Order repository
/// Inherits from IRepository<Order> but uses specific interface to avoid Scrutor auto-registration
/// </summary>
public interface IOrderDapperRepository : IRepository<Order>
{
}
