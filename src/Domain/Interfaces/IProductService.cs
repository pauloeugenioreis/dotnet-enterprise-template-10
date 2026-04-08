using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Product service interface with business logic
/// </summary>
public interface IProductService : IService<Product>
{
    Task<ProductResponseDto?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(bool? isActive, string? category, CancellationToken cancellationToken = default);

    Task<ProductResponseDto> CreateProductAsync(CreateProductRequest dto, CancellationToken cancellationToken = default);

    Task UpdateProductAsync(long id, UpdateProductRequest dto, CancellationToken cancellationToken = default);

    Task UpdateProductStatusAsync(long id, UpdateProductStatusRequest dto, CancellationToken cancellationToken = default);

    Task<ProductResponseDto> UpdateProductStockAsync(long id, UpdateProductStockRequest dto, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductResponseDto>> GetProductsForExportAsync(bool? isActive, string? category, CancellationToken cancellationToken = default);
}
