namespace ProjectTemplate.Domain.Events;

// ========================================
// ORDER EVENTS
// ========================================

/// <summary>
/// Event triggered when a new order is created
/// </summary>
public class OrderCreatedEvent
{
    public long OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public List<OrderItemData> Items { get; set; } = new();
    public string? Notes { get; set; }
}

/// <summary>
/// Event triggered when an order is updated
/// </summary>
public class OrderUpdatedEvent
{
    public long OrderId { get; set; }
    public Dictionary<string, object> Changes { get; set; } = new();
    public string? Reason { get; set; }
}

/// <summary>
/// Event triggered when an order status changes
/// </summary>
public class OrderStatusChangedEvent
{
    public long OrderId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// Event triggered when an order is shipped
/// </summary>
public class OrderShippedEvent
{
    public long OrderId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime ShippedAt { get; set; }
}

/// <summary>
/// Event triggered when an order is delivered
/// </summary>
public class OrderDeliveredEvent
{
    public long OrderId { get; set; }
    public DateTime DeliveredAt { get; set; }
    public string? ReceivedBy { get; set; }
}

/// <summary>
/// Event triggered when an order is cancelled
/// </summary>
public class OrderCancelledEvent
{
    public long OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? CancelledBy { get; set; }
}

/// <summary>
/// Event triggered when an order is deleted
/// </summary>
public class OrderDeletedEvent
{
    public long OrderId { get; set; }
    public string? Reason { get; set; }
}

// ========================================
// PRODUCT EVENTS
// ========================================

/// <summary>
/// Event triggered when a new product is created
/// </summary>
public class ProductCreatedEvent
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Event triggered when a product is updated
/// </summary>
public class ProductUpdatedEvent
{
    public long ProductId { get; set; }
    public Dictionary<string, object> Changes { get; set; } = new();
}

/// <summary>
/// Event triggered when product stock changes
/// </summary>
public class ProductStockChangedEvent
{
    public long ProductId { get; set; }
    public int OldQuantity { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; } = string.Empty; // "Order", "Restock", "Adjustment"
}

/// <summary>
/// Event triggered when a product price changes
/// </summary>
public class ProductPriceChangedEvent
{
    public long ProductId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Event triggered when a product is deleted
/// </summary>
public class ProductDeletedEvent
{
    public long ProductId { get; set; }
    public string? Reason { get; set; }
}

// ========================================
// SUPPORTING TYPES
// ========================================

/// <summary>
/// Order item data for events
/// </summary>
public class OrderItemData
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
