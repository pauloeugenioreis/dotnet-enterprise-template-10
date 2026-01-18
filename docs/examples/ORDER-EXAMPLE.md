# Order Example - Advanced Implementation

This example demonstrates **advanced Clean Architecture patterns** with a custom repository, business-focused service layer, and DTO-first API contracts.

## 📋 Overview

The Order example showcases:

- ✅ Custom repository queries (status, customer, date range)
- ✅ Dedicated service encapsulating business rules
- ✅ DTOs/records for request/response separation
- ✅ FluentValidation for strong input validation
- ✅ Business rules (stock validation, free shipping rules)
- ✅ Excel export with MiniExcel
- ✅ Complete CRUD surface
- ✅ Statistics and reporting endpoints

## 🏗️ Architecture

### Entity Layer (Domain)

**Order.cs** – Aggregate root with business properties:

```csharp
public class Order : EntityBase
{
    public string OrderNumber { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string ShippingAddress { get; set; }
    public string Status { get; set; } // Pending, Processing, Shipped, Delivered, Cancelled
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
```

**OrderItem.cs** – Order line items:

```csharp
public class OrderItem : EntityBase
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
```

### DTOs Layer (Domain/Dtos)

Records keep API contracts concise and immutable:

```csharp
// Request
public record CreateOrderDto
{
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public string? Phone { get; init; }
    public required string ShippingAddress { get; init; }
    public required List<OrderItemDto> Items { get; init; }
    public string? Notes { get; init; }
}

// Response
public record OrderResponseDto
{
    public long Id { get; init; }
    public string OrderNumber { get; init; }
    public string CustomerName { get; init; }
    public string Status { get; init; }
    public List<OrderItemResponseDto> Items { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Tax { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal Total { get; init; }
    // ... more properties
}
```

### Repository Layer (Data/Repository)

**IOrderRepository** extends the generic repository:

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email, CancellationToken ct);
    Task<IEnumerable<Order>> GetByStatusAsync(string status, CancellationToken ct);
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct);
}
```

**OrderRepository** provides EF Core implementations:

```csharp
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public async Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email, CancellationToken ct)
    {
        return await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o => o.CustomerEmail == email)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    // ... other custom queries
}
```

### Service Layer (Application/Services)

**IOrderService** adds business operations on top of `IService<Order>`:

```csharp
public interface IOrderService : IService<Order>
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct);
    Task<OrderResponseDto> UpdateOrderStatusAsync(long id, UpdateOrderStatusDto dto, CancellationToken ct);
    Task<OrderResponseDto?> GetOrderDetailsAsync(long id, CancellationToken ct);
    Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerAsync(string email, CancellationToken ct);
    Task<IEnumerable<OrderResponseDto>> GetOrdersByStatusAsync(string status, CancellationToken ct);
    Task<decimal> CalculateOrderTotalAsync(long orderId, CancellationToken ct);
}
```

**OrderService** encapsulates business logic:

```csharp
public class OrderService : Service<Order>, IOrderService
{
    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ValidationException("Order must have at least one item");

        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId, ct)
                ?? throw new NotFoundException($"Product {itemDto.ProductId} not found");

            if (!product.IsActive)
                throw new BusinessException($"Product {product.Name} is not available");

            if (product.Stock < itemDto.Quantity)
                throw new BusinessException($"Insufficient stock for {product.Name}");
        }

        var order = MapToEntity(dto);
        var subtotal = order.Items.Sum(i => i.Subtotal);
        order.Subtotal = subtotal;
        order.Tax = subtotal * 0.10m;
        order.ShippingCost = subtotal > 100 ? 0 : 10;
        order.Total = order.Subtotal + order.Tax + order.ShippingCost;

        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId, ct);
            product!.Stock -= itemDto.Quantity;
            await _productRepository.UpdateAsync(product.Id, product, ct);
        }

        var created = await _orderRepository.CreateAsync(order, ct);
        return MapToResponseDto(created);
    }
}
```

### Validation Layer (Domain/Validators)

```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Must not exceed 200 characters");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000");
        });
    }
}
```

### Controller Layer (Api/Controllers)

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrderController : ApiControllerBase
{
    private readonly IOrderService _orderService;

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateOrderDto dto, CancellationToken ct)
    {
        var order = await _orderService.CreateOrderAsync(dto, ct);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = order.Id }, order);
    }

    [HttpGet("ExportToExcel")]
    public async Task<IActionResult> ExportToExcelAsync(string? status, CancellationToken ct)
    {
        var orders = await GetFilteredOrdersAsync(status, ct);
        var excelData = orders.SelectMany(o => o.Items.Select(i => new
        {
            OrderId = o.Id,
            o.OrderNumber,
            o.CustomerName,
            ProductName = i.ProductName,
            i.Quantity,
            i.UnitPrice,
            OrderTotal = o.Total
        }));

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(excelData, configuration: config);

        return File(
            memoryStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}
```

## 🎯 Key Features

### 1. Custom Repository Pattern

```csharp
// Instead of generic:
var orders = await _repository.GetAllAsync();

// Use custom methods:
var orders = await _orderRepository.GetByCustomerEmailAsync("customer@email.com");
var pendingOrders = await _orderRepository.GetByStatusAsync("Pending");
```

### 2. Business Logic in Service

- ✅ Stock validation before order creation
- ✅ Automatic total calculation (subtotal + tax + shipping)
- ✅ Free shipping rules (orders > $100)
- ✅ Product availability checks
- ✅ Automatic order number generation
- ✅ Stock reduction after order creation

### 3. DTOs for API Contracts

**Separation of concerns:**

- `CreateOrderDto` – request payload
- `UpdateOrderStatusDto` – partial updates
- `OrderResponseDto` – response payload
- `OrderItemDto` – nested details

**Benefits:**

- Avoid exposing internal entities
- Enable API versioning and documentation
- Simplify validation and testing

### 4. FluentValidation Integration

```http
POST /api/v1/Order
Content-Type: application/json

{
  "customerName": "",
  "customerEmail": "invalid-email"
}
```

Response

```json
{
  "error": true,
  "messages": [
    "Customer name is required",
    "Invalid email format",
    "Order must have at least one item"
  ]
}
```

### 5. Excel Export with MiniExcel

```http
GET /api/v1/Order/ExportToExcel?status=Delivered
```

Returns an Excel file containing:

```text
OrderId | OrderNumber | CustomerName | ProductName | Quantity | UnitPrice | OrderTotal
```

### 6. Statistics and Reporting

```http
GET /api/v1/Order/statistics
```

```json
{
  "totalOrders": 150,
  "totalRevenue": 45678.90,
  "averageOrderValue": 304.52,
  "ordersByStatus": [
    { "status": "Delivered", "count": 120, "revenue": 40000 },
    { "status": "Pending", "count": 30, "revenue": 5678.90 }
  ],
  "topProducts": [
    { "productId": 5, "productName": "Product A", "quantitySold": 500, "revenue": 15000 }
  ]
}
```

## 📊 API Endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/v1/Order` | List orders (optional status filter) |
| GET | `/api/v1/Order/{id}` | Get order with full details |
| GET | `/api/v1/Order/customer/{email}` | Orders for a customer |
| POST | `/api/v1/Order` | Create a new order |
| PATCH | `/api/v1/Order/{id}/status` | Update order status |
| POST | `/api/v1/Order/{id}/cancel` | Cancel an order |
| GET | `/api/v1/Order/ExportToExcel` | Export to Excel |
| GET | `/api/v1/Order/statistics` | Order statistics |

## 🔄 Order Lifecycle

```text
Pending → Processing → Shipped → Delivered
         ↓
      Cancelled
```

## 💡 Best Practices Demonstrated

### 1. Separation of Concerns

- Entities ≠ DTOs ≠ API responses
- Controllers stay thin; services own business rules
- Repositories encapsulate persistence logic

### 2. SOLID Principles

- **S**ingle Responsibility – each class has one job
- **O**pen/Closed – generic services extended via composition
- **L**iskov – interfaces implement substitutable behavior
- **I**nterface Segregation – specialized repository/service interfaces
- **D**ependency Inversion – depend on abstractions

### 3. Repository Pattern

```csharp
// Generic for simple CRUD
IRepository<Product> _productRepository;

// Custom for complex queries
IOrderRepository _orderRepository;
```

### 4. Service Layer Pattern

```csharp
// Avoid putting business logic in controllers
public async Task<IActionResult> CreateOrder(Order order)
{
    if (order.Items.Count == 0) return BadRequest();
    // ... more logic
}
```

```csharp
// Keep logic in the service instead
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    var order = await _orderService.CreateOrderAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
}
```

### 5. DTO Pattern

```csharp
// Avoid exposing entities directly
public async Task<IActionResult> Get() => Ok(await _repository.GetAllAsync());
```

```csharp
// Return DTOs mapped in service layer
public async Task<IActionResult> Get() => Ok(await _service.GetOrdersAsync());
```

### 6. Validation

- FluentValidation registered in DI
- Validation occurs before controller action executes

### 7. Error Handling

```csharp
throw new ValidationException("Order must have at least one item");
throw new NotFoundException($"Product {id} not found");
throw new BusinessException($"Insufficient stock for {productName}");
```

Global exception middleware maps exceptions to HTTP status codes (400, 404, 422).

### 8. Async/Await Everywhere

```csharp
Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct);
```

Every method accepts `CancellationToken` and uses `await` correctly.

### 9. Logging & Monitoring

```csharp
_logger.LogInformation(
    "Order {OrderNumber} created for {CustomerEmail}. Total: {Total:C}",
    order.OrderNumber,
    order.CustomerEmail,
    order.Total);
```

```csharp
var stopwatch = Stopwatch.StartNew();
// ... operation
stopwatch.Stop();
return Ok(new { executionTime = $"{stopwatch.ElapsedMilliseconds}ms", data });
```

## 🆚 Comparison: Product vs Order

| Feature | Product (Basic) | Order (Advanced) |
| --- | --- | --- |
| Repository | Generic `IRepository<Product>` | Custom `IOrderRepository` |
| Service | Generic `IService<Product>` | Custom `IOrderService` |
| DTOs | Not required | Request/response DTOs |
| Validation | ModelState | FluentValidation rules |
| Business Logic | Minimal | Stock, totals, status transitions |
| Relationships | None | `Order` owns `OrderItems` |
| Custom Queries | No | Yes (customer, status, date range) |

## 📝 Usage Example

### Create Order Request

```http
POST /api/v1/Order
Content-Type: application/json

{
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "phone": "+1234567890",
  "shippingAddress": "123 Main St, City, State 12345",
  "items": [
    { "productId": 1, "quantity": 2, "unitPrice": 29.99 },
    { "productId": 3, "quantity": 1, "unitPrice": 49.99 }
  ],
  "notes": "Please deliver after 6 PM"
}
```

### Response

```json
{
  "id": 42,
  "orderNumber": "ORD-20260111-A3B4C5D6",
  "customerName": "John Doe",
  "customerEmail":"john@example.com",
  "customerPhone": "+1234567890",
  "shippingAddress": "123 Main St, City, State 12345",
  "status": "Pending",
  "items": [
    {
      "id": 101,
      "productId": 1,
      "productName": "Product One",
      "quantity": 2,
      "unitPrice": 29.99,
      "subtotal": 59.98
    },
    {
      "id": 102,
      "productId": 3,
      "productName": "Product Three",
      "quantity": 1,
      "unitPrice": 49.99,
      "subtotal": 49.99
    }
  ],
  "subtotal": 109.97,
  "tax": 10.99,
  "shippingCost": 0.00,
  "total": 120.96,
  "notes": "Please deliver after 6 PM",
  "createdAt": "2026-01-11T10:30:00Z",
  "updatedAt": null
}
```

## 🎓 Learning Takeaways

1. **When to prefer custom implementations** – Use generic services for CRUD, custom ones for complex business logic.
2. **DTOs protect API contracts** – Hide internal structures, support versioning, and strengthen validation.
3. **Service layer enforces rules** – Central place for transactions, orchestration, and cross-cutting concerns.
4. **Repository pattern benefits** – Abstract persistence, simplify testing, and keep queries centralized.
5. **Records shine for DTOs** – Immutable by default, concise syntax, and value-based equality.

## 🚀 Next Steps

1. **Add domain events** – Publish `OrderCreated`/`OrderShipped` to RabbitMQ.
2. **Integrate payments** – Track payment status/methods and gateway callbacks.
3. **Enhance inventory** – Reserved stock, back-order handling, notifications.
4. **Send notifications** – Email confirmations, SMS updates, push notifications.
5. **Add tracking** – Carrier integrations, tracking numbers, real-time updates.

---
**This example demonstrates production-ready Clean Architecture with best practices for .NET 10!** 🎯
