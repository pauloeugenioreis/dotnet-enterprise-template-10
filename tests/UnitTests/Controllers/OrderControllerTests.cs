using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Api.Controllers;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;
using Xunit;

using OrderItemDto = ProjectTemplate.Domain.Dtos.OrderItemDto;

namespace ProjectTemplate.UnitTests.Controllers;

/// <summary>
/// Unit tests for OrderController
/// Tests controller logic with mocked custom service
/// </summary>
public class OrderControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<ILogger<OrderController>> _mockLogger;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _mockLogger = new Mock<ILogger<OrderController>>();
        _controller = new OrderController(_mockOrderService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOkResult_WhenOrdersExist()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order 
            { 
                Id = 1, 
                OrderNumber = "ORD-001", 
                CustomerName = "João Silva",
                CustomerEmail = "joao@email.com",
                ShippingAddress = "Rua A, 123",
                Status = "Pending",
                Total = 100.00m
            }
        };
        var orderDetails = new OrderResponseDto
        {
            Id = 1,
            OrderNumber = "ORD-001",
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            CustomerPhone = "(11) 99999-9999",
            ShippingAddress = "Rua A, 123",
            Status = "Pending",
            Subtotal = 90.00m,
            Tax = 5.00m,
            ShippingCost = 5.00m,
            Total = 100.00m,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItemResponseDto>()
        };
        _mockOrderService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(orders);
        _mockOrderService.Setup(s => s.GetOrderDetailsAsync(1L, It.IsAny<CancellationToken>())).ReturnsAsync(orderDetails);

        // Act
        var result = await _controller.GetAllAsync(null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var orderId = 1L;
        var orderResponse = new OrderResponseDto
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            CustomerPhone = "(11) 99999-9999",
            ShippingAddress = "Rua A, 123",
            Status = "Pending",
            Subtotal = 100.00m,
            Tax = 10.00m,
            ShippingCost = 15.00m,
            Total = 125.00m,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItemResponseDto>()
        };
        _mockOrderService.Setup(s => s.GetOrderDetailsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(orderResponse);

        // Act
        var result = await _controller.GetByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var orderId = 999L;
        _mockOrderService.Setup(s => s.GetOrderDetailsAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((OrderResponseDto?)null);

        // Act
        var result = await _controller.GetByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateAsync_WithValidOrder_ReturnsCreatedAtAction()
    {
        // Arrange
        var createRequest = new CreateOrderRequest
        {
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            Phone = "(11) 99999-9999",
            ShippingAddress = "Rua A, 123",
            Items = new List<OrderItemDto>
            {
                new OrderItemDto(1, 2, 50.00m)
            },
            Notes = null
        };

        var createdOrder = new OrderResponseDto
        {
            Id = 1,
            OrderNumber = "ORD-001",
            CustomerName = createRequest.CustomerName,
            CustomerEmail = createRequest.CustomerEmail,
            CustomerPhone = createRequest.Phone,
            ShippingAddress = createRequest.ShippingAddress,
            Status = "Pending",
            Subtotal = 100.00m,
            Tax = 10.00m,
            ShippingCost = 15.00m,
            Total = 125.00m,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItemResponseDto>()
        };

        _mockOrderService.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdOrder);

        // Act
        var result = await _controller.CreateAsync(createRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var orderId = 1L;
        var updateRequest = new UpdateOrderStatusDto("Shipped", "Order dispatched");
        var updatedOrder = new OrderResponseDto
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            CustomerPhone = "(11) 99999-9999",
            ShippingAddress = "Rua A, 123",
            Status = "Shipped",
            Subtotal = 100.00m,
            Tax = 10.00m,
            ShippingCost = 15.00m,
            Total = 125.00m,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItemResponseDto>()
        };

        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, It.IsAny<UpdateOrderStatusDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _controller.UpdateStatusAsync(orderId, updateRequest, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CancelOrderAsync_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var orderId = 1L;
        var cancelledOrder = new OrderResponseDto
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            CustomerPhone = "(11) 99999-9999",
            ShippingAddress = "Rua A, 123",
            Status = "Cancelled",
            Subtotal = 100.00m,
            Tax = 10.00m,
            ShippingCost = 15.00m,
            Total = 125.00m,
            Notes = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<OrderItemResponseDto>()
        };

        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, It.IsAny<UpdateOrderStatusDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cancelledOrder);

        // Act
        var result = await _controller.CancelOrderAsync(orderId, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByCustomerAsync_WithValidEmail_ReturnsOkResult()
    {
        // Arrange
        var email = "joao@email.com";
        var orders = new List<OrderResponseDto>
        {
            new OrderResponseDto 
            { 
                Id = 1, 
                OrderNumber = "ORD-001", 
                CustomerEmail = email,
                CustomerName = "João Silva",
                Status = "Delivered",
                Total = 100.00m,
                ShippingAddress = "Test",
                Subtotal = 90m,
                Tax = 10m,
                ShippingCost = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<OrderItemResponseDto>()
            }
        };

        _mockOrderService.Setup(s => s.GetOrdersByCustomerAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        // Act
        var result = await _controller.GetByCustomerAsync(email, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
