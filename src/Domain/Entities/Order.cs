using System.ComponentModel.DataAnnotations;namespace ProjectTemplate.Domain.Entities;












































































































































}    public DateTime? DeliveredAt { get; set; }    public DateTime? ShippedAt { get; set; }    public bool IsPaid { get; set; }    public string? Notes { get; set; }    public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled    public decimal TotalAmount { get; set; }    public string CustomerEmail { get; set; } = string.Empty;    public string CustomerName { get; set; } = string.Empty;    public DateTime OrderDate { get; set; } = DateTime.UtcNow;    public string OrderNumber { get; set; } = string.Empty;{public class Order : EntityBase/// </summary>/// Order entity representing a customer order/// <summary><parameter name="content">namespace ProjectTemplate.Domain.Entities;<invoke name="create_file"><function_calls>- ✅ Validações- ✅ Excel export- ✅ Controller completo- ✅ Service customizado com regras de negócio- ✅ Repository customizado- ✅ DTOs (records) para Create/Update/Response- ✅ Entidade Order + OrderItemVou criar um exemplo completo de **Order (Pedido)** com:```    decimal TotalPrice);    decimal UnitPrice,    int Quantity,    string ProductName,    long ProductId,public record OrderItemDto(}    public IEnumerable<OrderItemDto> Items { get; init; } = [];    public DateTime CreatedAt { get; init; }    public int ItemsCount { get; init; }    public decimal TotalAmount { get; init; }    public string Status { get; set; } = string.Empty;    public string CustomerEmail { get; set; } = string.Empty;    public string CustomerName { get; init; } = string.Empty;    public long Id { get; init; }{public record OrderResponse/// </summary>/// Response model for order with calculated totals/// <summary>);    decimal UnitPrice    int Quantity,    long ProductId,public record OrderItemRequest(/// </summary>/// Request model for order items/// <summary>);    List<OrderItemRequest> Items    string CustomerEmail,    string CustomerName,public record CreateOrderRequest(/// </summary>/// Request model for creating orders/// <summary>);    decimal TotalPrice    decimal UnitPrice,    int Quantity,    string ProductName,    long ProductId,    long Id,public record OrderItemResponse(/// </summary>/// DTO for order item details/// <summary>);    DateTime? UpdatedAt    DateTime CreatedAt,    ICollection<OrderItemResponse> Items,    string Status,    decimal TotalAmount,    DateTime OrderDate,    string CustomerEmail,    string CustomerName,    long Id,public record OrderResponse(/// </summary>/// Response DTO for orders/// <summary>);    decimal UnitPrice    int Quantity,    long ProductId,public record OrderItemDto(/// </summary>/// DTO for order items/// <summary>);    List<OrderItemDto> Items    string? Notes,    long Id,public record UpdateOrderDto(/// </summary>/// DTO for updating an order/// <summary>}    public List<OrderItemDto> Items { get; init; } = new();    /// </summary>    /// Order items    /// <summary>    public required string Email { get; init; }    /// </summary>    /// Customer email    /// <summary>    public required string CustomerName { get; init; }    /// </summary>    /// Customer name    /// <summary>{public record CreateOrderDto/// </summary>/// DTO for creating a new order/// <summary>namespace ProjectTemplate.Domain.Dtos;
/// <summary>
/// Order entity - represents a customer order
/// </summary>
public class Order : EntityBase
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Notes { get; set; }

    // Navigation property for items
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

/// <summary>
/// Order item entity
/// </summary>
public class OrderItem : EntityBase
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    
    // Navigation
    public Order? Order { get; set; }
}
