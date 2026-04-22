using Microsoft.Extensions.Logging;
using ProjectTemplate.SharedModels;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Application.Services;

/// <summary>
/// Product service with business logic, filtering and mapping
/// </summary>
public class ProductService(
    IProductRepository repository,
    ILogger<ProductService> logger) : Service<Product>(repository, logger), IProductService
{
    public async Task<ProductResponseDto?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default)
    {

        var product = await repository.GetByIdAsync(id, cancellationToken);
        return product is not null ? MapToResponse(product) : null;
    }

    public async Task<(IEnumerable<ProductResponseDto> Items, int Total)> GetAllProductsAsync(
        bool? isActive,
        string? category,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var (products, total) = await repository.GetByFilterAsync(isActive, category, page, pageSize, cancellationToken);
        return (products.Select(MapToResponse).ToList(), total);
    }

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductRequest dto, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Category = dto.Category,
            IsActive = dto.IsActive
        };

        var created = await repository.AddAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product {ProductName} created with ID {ProductId}", created.Name, created.Id);

        return MapToResponse(created);
    }

    public async Task UpdateProductAsync(long id, UpdateProductRequest dto, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {id} not found");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.Category = dto.Category;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product {ProductId} updated", id);
    }

    public async Task UpdateProductStatusAsync(long id, UpdateProductStatusRequest dto, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {id} not found");

        product.IsActive = dto.IsActive ?? product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product {ProductId} status changed to {Status}",
            id, product.IsActive ? "active" : "inactive");
    }

    public async Task<ProductResponseDto> UpdateProductStockAsync(long id, UpdateProductStockRequest dto, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {id} not found");

        var newStock = product.Stock + dto.Quantity;

        if (newStock < 0)
        {
            throw new BusinessException(
                $"Insufficient stock. Current: {product.Stock}, requested change: {dto.Quantity}, resulting: {newStock}");
        }

        var oldStock = product.Stock;
        product.Stock = newStock;
        product.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product {ProductId} stock updated: {OldStock} -> {NewStock} ({Change})",
            id, oldStock, newStock,
            dto.Quantity > 0 ? $"+{dto.Quantity}" : dto.Quantity.ToString());

        return MapToResponse(product);
    }

    private static ProductResponseDto MapToResponse(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Stock = product.Stock,
        Category = product.Category,
        IsActive = product.IsActive,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}
