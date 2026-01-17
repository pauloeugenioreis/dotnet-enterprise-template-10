# Order Example - Advanced Implementation

This example demonstrates **advanced Clean Architecture patterns** with custom repository, custom service, and proper DTO usage.

## 📋 Overview

The Order example showcases:
- ✅ **Custom Repository** with specialized queries
- ✅ **Custom Service** with business logic
- ✅ **DTOs/Records** for request/response separation
- ✅ **FluentValidation** for input validation
- ✅ **Business Rules** (stock validation, total calculation)
- ✅ **Excel Export** with order details
- ✅ **Complete CRUD** operations
- ✅ **Statistics** and reporting

## 🏗️ Architecture

### Entity Layer (Domain)

**Order.cs** - Main entity with business properties

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
    
    public ICollection<OrderItem> Items { get; set; }
}
```csharp

**OrderItem.cs** - Order line items

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
### DTOs Layer (Domain/Dtos)

**Records** for immutability and conciseness:

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
### Repository Layer (Data/Repository)

**IOrderRepository** extends generic repository with custom methods:

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email, CancellationToken ct);
    Task<IEnumerable<Order>> GetByStatusAsync(string status, CancellationToken ct);
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct);
}
**OrderRepository** implements with EF Core:

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
    // ... more custom methods
}
### Service Layer (Application/Services)

**IOrderService** extends generic service with business methods:

public interface IOrderService : IService<Order>
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct);
    Task<OrderResponseDto> UpdateOrderStatusAsync(long id, UpdateOrderStatusDto dto, CancellationToken ct);
    Task<OrderResponseDto?> GetOrderDetailsAsync(long id, CancellationToken ct);
    Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerAsync(string email, CancellationToken ct);
    Task<IEnumerable<OrderResponseDto>> GetOrdersByStatusAsync(string status, CancellationToken ct);
    Task<decimal> CalculateOrderTotalAsync(long orderId, CancellationToken ct);
}
**OrderService** implements with business logic:

public class OrderService : Service<Order>, IOrderService
{
    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct)
    {
        // 1. Validate items
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ValidationException("Order must have at least one item");

        // 2. Check product availability and stock
        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId, ct);
            
            if (product == null)
                throw new NotFoundException($"Product {itemDto.ProductId} not found");
            
            if (!product.IsActive)
                throw new BusinessException($"Product {product.Name} is not available");
            
            if (product.Stock < itemDto.Quantity)
                throw new BusinessException($"Insufficient stock for {product.Name}");
        }

        // 3. Calculate totals
        order.Subtotal = subtotal;
        order.Tax = subtotal * 0.10m; // 10% tax
        order.ShippingCost = subtotal > 100 ? 0 : 10.00m; // Free shipping over $100
        order.Total = order.Subtotal + order.Tax + order.ShippingCost;

        // 4. Update product stock
        product.Stock -= itemDto.Quantity;
        await _productRepository.UpdateAsync(product.Id, product, ct);

        // 5. Save order
        var createdOrder = await _orderRepository.CreateAsync(order, ct);

        return MapToResponseDto(createdOrder);
    }
}
### Validation Layer (Domain/Validators)

**FluentValidation** for automatic validation:

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
### Controller Layer (Api/Controllers)

**OrderController** with comprehensive endpoints:

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrderController : ApiControllerBase
{
    private readonly IOrderService _orderService;

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.CreateOrderAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = order.Id }, order);
    }

    [HttpGet("ExportToExcel")]
    public async Task<ActionResult> ExportToExcelAsync(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var orders = await GetFilteredOrdersAsync(status, cancellationToken);
        
        var excelData = orders.SelectMany(o => o.Items.Select(i => new
        {
            OrderId = o.Id,
            o.OrderNumber,
            o.CustomerName,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            OrderTotal = o.Total
        }));

        var memoryStream = new MemoryStream();
        await memoryStream.SaveAsAsync(excelData, configuration: config);
        
        return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}
## 🎯 Key Features

### 1. **Custom Repository Pattern**

// Instead of generic:
var orders = await _repository.GetAllAsync();

// Use custom methods:
var orders = await _orderRepository.GetByCustomerEmailAsync("customer@email.com");
var pendingOrders = await _orderRepository.GetByStatusAsync("Pending");
### 2. **Business Logic in Service**

- ✅ Stock validation before order creation
- ✅ Automatic total calculation (subtotal + tax + shipping)
- ✅ Free shipping rules ($100+ = free)
- ✅ Product availability check
- ✅ Automatic order number generation
- ✅ Stock reduction after order creation

### 3. **DTOs for API Contracts**

**Separation of concerns:**
- `CreateOrderDto` - What client sends
- `UpdateOrderStatusDto` - Partial updates
- `OrderResponseDto` - What API returns
- `OrderItemDto` - Nested item details

**Benefits:**
- ✅ Don't expose internal entity structure
- ✅ Can evolve API independently
- ✅ Better documentation
- ✅ Easier versioning

### 4. **FluentValidation Integration**

Automatic validation before controller action:

POST /api/v1/Order
{
  "customerName": "",
  "customerEmail": "invalid-email"
}

Response: 400 Bad Request
{
  "error": true,
  "messages": [
    "Customer name is required",
    "Invalid email format",
    "Order must have at least one item"
  ]
}
### 5. **Excel Export with MiniExcel**

GET /api/v1/Order/ExportToExcel?status=Delivered

// Returns Excel file with:
// OrderId | OrderNumber | CustomerName | ProductName | Quantity | UnitPrice | OrderTotal
### 6. **Statistics and Reporting**

GET /api/v1/Order/statistics

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
## 📊 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/Order` | Get all orders (with optional status filter) |
| GET | `/api/v1/Order/{id}` | Get order by ID with full details |
| GET | `/api/v1/Order/customer/{email}` | Get all orders for a customer |
| POST | `/api/v1/Order` | Create new order |
| PATCH | `/api/v1/Order/{id}/status` | Update order status |
| POST | `/api/v1/Order/{id}/cancel` | Cancel order |
| GET | `/api/v1/Order/ExportToExcel` | Export orders to Excel |
| GET | `/api/v1/Order/statistics` | Get order statistics |

## 🔄 Order Lifecycle

```csharp
Pending → Processing → Shipped → Delivered
   ↓
Cancelled (at any stage)
## 💡 Best Practices Demonstrated

### 1. **Separation of Concerns**
- Entity ≠ DTO ≠ API Response
- Each layer has its responsibility
- Business logic in Service, not Controller

### 2. **SOLID Principles**
- **S**ingle Responsibility: Each class has one job
- **O**pen/Closed: Extend generic repository/service
- **L**iskov Substitution: IOrderService implements IService
- **I**nterface Segregation: Custom interfaces for specific needs
- **D**ependency Inversion: Depend on abstractions (interfaces)

### 3. **Repository Pattern**
// Generic for simple CRUD
IRepository<Product> _productRepository;

// Custom for complex queries
IOrderRepository _orderRepository;
### 4. **Service Layer Pattern**
// Don't put business logic in controller:
❌ public async Task<IActionResult> CreateOrder(Order order)
{
    if (order.Items.Count == 0) return BadRequest();
    // ... more logic in controller
}

// Put business logic in service:
✅ public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    var order = await _orderService.CreateOrderAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
}
### 5. **DTO Pattern**
// Don't expose entities directly:
❌ public async Task<IActionResult> Get() => Ok(await _repository.GetAllAsync());

// Use DTOs for API contracts:
✅ public async Task<IActionResult> Get() => Ok(await _service.GetOrdersAsync());
### 6. **Validation**
// Automatic validation with FluentValidation
// Validators registered in DI container
// Validation happens before controller action
### 7. **Error Handling**
// Custom exceptions for domain errors
throw new ValidationException("Order must have at least one item");
throw new NotFoundException($"Product {id} not found");
throw new BusinessException($"Insufficient stock for {productName}");

// Global exception handler converts to HTTP responses
// 400 for ValidationException
// 404 for NotFoundException
// 422 for BusinessException
### 8. **Async/Await**
// All methods support CancellationToken
// Proper async implementation throughout
Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct);
### 9. **Logging**
_logger.LogInformation("Order {OrderNumber} created for {CustomerEmail}. Total: {Total:C}",
    order.OrderNumber, order.CustomerEmail, order.Total);
### 10. **Performance Monitoring**
var stopwatch = Stopwatch.StartNew();
// ... operation
stopwatch.Stop();
return Ok(new { executionTime = $"{stopwatch.ElapsedMilliseconds}ms", data });
## 🆚 Comparison: Product vs Order

| Feature | Product (Basic) | Order (Advanced) |
|---------|----------------|------------------|
| Repository | Generic `IRepository<Product>` | Custom `IOrderRepository : IRepository<Order>` |
| Service | Generic `IService<Product>` | Custom `IOrderService : IService<Order>` |
| DTOs | No (uses entity directly) | Yes (Request/Response records) |
| Validation | Basic ModelState | FluentValidation with custom rules |
| Business Logic | Minimal | Complex (stock, totals, status transitions) |
| Relationships | None | Has OrderItems collection |
| Custom Queries | No | Yes (by customer, status, date range) |

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
    {
      "productId": 1,
      "quantity": 2,
      "unitPrice": 29.99
    },
    {
      "productId": 3,
      "quantity": 1,
      "unitPrice": 49.99
    }
  ],
  "notes": "Please deliver after 6 PM"
}
### Response

{
  "id": 42,
  "orderNumber": "ORD-20260111-A3B4C5D6",
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
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

1. **When to use generic vs custom implementations:**
   - Generic: Simple CRUD operations
   - Custom: Complex queries or business logic

2. **DTOs are essential for:**
   - Hiding internal structure
   - API versioning
   - Input validation
   - Response shaping

3. **Service layer is crucial for:**
   - Business rules enforcement
   - Transaction coordination
   - Cross-cutting concerns
   - Testability

4. **Repository pattern benefits:**
   - Abstraction over data access
   - Easier testing (mock repositories)
   - Centralized query logic
   - ORM independence

5. **Records for DTOs:**
   - Immutable by default
   - Concise syntax
   - Value equality
   - Perfect for API contracts

## 🚀 Next Steps

Want to extend this example? Try:

1. **Add Order Events**
   - OrderCreated, OrderShipped events
   - Publish to RabbitMQ

2. **Add Payment Processing**
   - Payment method
   - Payment status
   - Payment gateway integration

3. **Add Inventory Management**
   - Reserved stock
   - Back-order handling
   - Stock notifications

4. **Add Notifications**
   - Email confirmation
   - SMS updates
   - Push notifications

5. **Add Order Tracking**
   - Shipping carrier integration
   - Tracking number
   - Real-time status updates

---

**This example demonstrates production-ready Clean Architecture with best practices for .NET 10!** 🎯
