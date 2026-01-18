using Microsoft.Extensions.Logging;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Application.Services;

/// <summary>
/// Order service with business logic and validation
/// </summary>
public class OrderService : Service<Order>, IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IRepository<Product> productRepository,
        ILogger<OrderService> logger)
        : base(orderRepository, logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderRequest dto, CancellationToken cancellationToken = default)
    {
        // Validate items
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new ValidationException("Order must have at least one item");
        }

        // Create order entity
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.Phone,
            ShippingAddress = dto.ShippingAddress,
            Status = OrderStatus.Pending,
            Notes = dto.Notes,
            Items = new List<OrderItem>()
        };

        decimal subtotal = 0;

        // Process items
        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException($"Product {itemDto.ProductId} not found");
            }

            if (!product.IsActive)
            {
                throw new BusinessException($"Product {product.Name} is not available");
            }

            if (product.Stock < itemDto.Quantity)
            {
                throw new BusinessException($"Insufficient stock for {product.Name}. Available: {product.Stock}");
            }

            var itemSubtotal = itemDto.UnitPrice * itemDto.Quantity;
            subtotal += itemSubtotal;

            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                ProductName = product.Name,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                Subtotal = itemSubtotal
            };

            order.Items.Add(orderItem);

            // Update product stock
            product.Stock -= itemDto.Quantity;
            await _productRepository.UpdateAsync(product, cancellationToken);
        }

        // Calculate totals
        order.Subtotal = subtotal;
        order.Tax = subtotal * 0.10m; // 10% tax
        order.ShippingCost = subtotal > 100 ? 0 : 10.00m; // Free shipping over $100
        order.Total = order.Subtotal + order.Tax + order.ShippingCost;

        // Save order
        var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} created for {CustomerEmail}. Total: {Total:C}",
            order.OrderNumber, order.CustomerEmail, order.Total);

        return MapToResponseDto(createdOrder);
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(long id, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException($"Order {id} not found");
        }

        var previousStatus = order.Status;
        if (!OrderStatus.TryNormalize(dto.Status, out var normalizedStatus))
        {
            throw new ValidationException(
                $"Status must be one of: {string.Join(", ", OrderStatus.AllowedStatuses)}");
        }

        order.Status = normalizedStatus;

        if (!string.IsNullOrEmpty(dto.Reason))
        {
            order.Notes = $"{order.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Status changed to {normalizedStatus}: {dto.Reason}";
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order {OrderNumber} status updated: {OldStatus} -> {NewStatus}",
            order.OrderNumber, previousStatus, normalizedStatus);

        return MapToResponseDto(order);
    }

    public async Task<OrderResponseDto?> GetOrderDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order != null ? MapToResponseDto(order) : null;
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByCustomerAsync(string email, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByCustomerEmailAsync(email, cancellationToken);
        return orders.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        if (!OrderStatus.TryNormalize(status, out var normalizedStatus))
        {
            throw new ValidationException(
                $"Status must be one of: {string.Join(", ", OrderStatus.AllowedStatuses)}");
        }

        var orders = await _orderRepository.GetByStatusAsync(normalizedStatus, cancellationToken);
        return orders.Select(MapToResponseDto);
    }

    public async Task<decimal> CalculateOrderTotalAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException($"Order {orderId} not found");
        }

        return order.Total;
    }

    private static OrderResponseDto MapToResponseDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            ShippingAddress = order.ShippingAddress,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList(),
            Subtotal = order.Subtotal,
            Tax = order.Tax,
            ShippingCost = order.ShippingCost,
            Total = order.Total,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
