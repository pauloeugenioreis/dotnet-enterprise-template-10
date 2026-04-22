using System;

namespace ProjectTemplate.SharedModels;

/// <summary>
/// Unified request payload for creating or updating a product.
/// When Id is null, a new product is created. When Id has a value, the product is updated.
/// </summary>
public record SaveProductRequest
{
    /// <summary>
    /// Product Id — null for creation, set for update.
    /// </summary>
    public long? Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public required string Category { get; init; }
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Backward-compatible alias for creating a product.
/// </summary>
public record CreateProductRequest : SaveProductRequest;

/// <summary>
/// Backward-compatible alias for updating a product.
/// </summary>
public record UpdateProductRequest : SaveProductRequest;

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
