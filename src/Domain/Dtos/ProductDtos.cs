using System;

namespace ProjectTemplate.Domain.Dtos;

/// <summary>
/// Request payload for creating a product.
/// </summary>
public record CreateProductRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public required string Category { get; init; }
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Request payload for updating a product.
/// </summary>
public record UpdateProductRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public required string Category { get; init; }
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// DTO returned by the API when interacting with products.
/// </summary>
public record ProductResponseDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public string Category { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Request payload for toggling product active flag.
/// </summary>
public record UpdateProductStatusRequest
{
    public bool? IsActive { get; init; }
}

/// <summary>
/// Request payload for updating product stock levels.
/// </summary>
public record UpdateProductStockRequest
{
    /// <summary>
    /// Quantity to add (positive) or remove (negative) from stock.
    /// </summary>
    public int Quantity { get; init; }
}
