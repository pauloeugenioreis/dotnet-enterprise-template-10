using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Entities;

/// <summary>
/// Product entity example
/// </summary>
public class Product : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public new bool IsActive { get; set; } = true;  // Override EntityBase.IsActive
}
