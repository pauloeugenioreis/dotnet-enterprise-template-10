using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Api.Controllers;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;
using Xunit;

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
    public async Task GetAll_ReturnsOkResult_WithListOfOrders()
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
            },
            new Order 
            { 
                Id = 2, 
                OrderNumber = "ORD-002", 
                CustomerName = "Maria Santos",
                CustomerEmail = "maria@email.com",
                ShippingAddress = "Rua B, 456",
                Status = "Delivered",
                Total = 200.00m
            }
        };
        _mockOrderService.Setup(s => s.GetAllAsync()).ReturnsAsync(orders);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<Order>>().Subject;
        returnedOrders.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResult_WithOrderDetails()
    {
        // Arrange
        var orderId = 1L;
        var orderResponse = new OrderResponseDto(
            Id: orderId,
            OrderNumber: "ORD-001",
            CustomerName: "João Silva",
            CustomerEmail: "joao@email.com",
            CustomerPhone: "(11) 99999-9999",
            ShippingAddress: "Rua A, 123",
            Status: "Pending",
            Subtotal: 100.00m,
            Tax: 10.00m,
            ShippingCost: 15.00m,
            Total: 125.00m,
            Notes: null,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Items: new List<OrderItemResponseDto>()
        );
        _mockOrderService.Setup(s => s.GetOrderDetailsAsync(orderId)).ReturnsAsync(orderResponse);

        // Act
        var result = await _controller.GetById(orderId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrder = okResult.Value.Should().BeAssignableTo<OrderResponseDto>().Subject;
        returnedOrder.Id.Should().Be(orderId);
        returnedOrder.OrderNumber.Should().Be("ORD-001");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var orderId = 999L;
        _mockOrderService.Setup(s => s.GetOrderDetailsAsync(orderId)).ReturnsAsync((OrderResponseDto?)null);

        // Act
        var result = await _controller.GetById(orderId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidOrder_ReturnsCreatedAtAction()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(
            CustomerName: "João Silva",
            CustomerEmail: "joao@email.com",
            CustomerPhone: "(11) 99999-9999",
            ShippingAddress: "Rua A, 123",
            Items: new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(ProductId: 1, Quantity: 2, UnitPrice: 50.00m)
            },
            Notes: null
        );

        var createdOrder = new Order
        {
            Id = 1,
            OrderNumber = "ORD-001",
            CustomerName = createRequest.CustomerName,
            CustomerEmail = createRequest.CustomerEmail,
            CustomerPhone = createRequest.CustomerPhone,
            ShippingAddress = createRequest.ShippingAddress,
            Status = "Pending",
            Total = 125.00m
        };

        _mockOrderService.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderRequest>())).ReturnsAsync(createdOrder);

        // Act
        var result = await _controller.Create(createRequest);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(OrderController.GetById));
        var returnedOrder = createdResult.Value.Should().BeAssignableTo<Order>().Subject;
        returnedOrder.Id.Should().Be(1);
    }

    [Fact]
    public async Task UpdateStatus_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var orderId = 1L;
        var updateRequest = new UpdateOrderStatusDto("Shipped", "Order dispatched");
        var updatedOrder = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            ShippingAddress = "Rua A, 123",
            Status = "Shipped",
            Total = 125.00m
        };

        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, updateRequest.Status, updateRequest.Reason))
            .ReturnsAsync(updatedOrder);

        // Act
        var result = await _controller.UpdateStatus(orderId, updateRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrder = okResult.Value.Should().BeAssignableTo<Order>().Subject;
        returnedOrder.Status.Should().Be("Shipped");
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var orderId = 999L;
        var updateRequest = new UpdateOrderStatusDto("Shipped", "Order dispatched");
        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, updateRequest.Status, updateRequest.Reason))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _controller.UpdateStatus(orderId, updateRequest);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Cancel_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var orderId = 1L;
        var cancelledOrder = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            CustomerName = "João Silva",
            CustomerEmail = "joao@email.com",
            ShippingAddress = "Rua A, 123",
            Status = "Cancelled",
            Total = 125.00m
        };

        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, "Cancelled", "Cancelled by customer"))
            .ReturnsAsync(cancelledOrder);

        // Act
        var result = await _controller.Cancel(orderId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrder = okResult.Value.Should().BeAssignableTo<Order>().Subject;
        returnedOrder.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task GetByCustomer_WithValidEmail_ReturnsOkResult_WithOrders()
    {
        // Arrange
        var email = "joao@email.com";
        var orders = new List<Order>
        {
            new Order 
            { 
                Id = 1, 
                OrderNumber = "ORD-001", 
                CustomerEmail = email,
                CustomerName = "João Silva",
                Status = "Delivered",
                Total = 100.00m
            }
        };

        _mockOrderService.Setup(s => s.GetOrdersByCustomerAsync(email)).ReturnsAsync(orders);

        // Act
        var result = await _controller.GetByCustomer(email);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<Order>>().Subject;
        returnedOrders.Should().HaveCount(1);
        returnedOrders.First().CustomerEmail.Should().Be(email);
    }

    [Fact]
    public async Task GetByStatus_WithValidStatus_ReturnsOkResult_WithOrders()
    {
        // Arrange
        var status = "Pending";
        var orders = new List<Order>
        {
            new Order { Id = 1, OrderNumber = "ORD-001", Status = status, Total = 100.00m },
            new Order { Id = 2, OrderNumber = "ORD-002", Status = status, Total = 200.00m }
        };

        _mockOrderService.Setup(s => s.GetOrdersByStatusAsync(status)).ReturnsAsync(orders);

        // Act
        var result = await _controller.GetByStatus(status);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<Order>>().Subject;
        returnedOrders.Should().HaveCount(2);
        returnedOrders.Should().AllSatisfy(o => o.Status.Should().Be(status));
    }

    [Fact]
    public async Task GetStatistics_ReturnsOkResult_WithStatistics()
    {
        // Arrange
        var statistics = new
        {
            TotalOrders = 100,
            TotalRevenue = 10000.00m,
            AverageOrderValue = 100.00m,
            OrdersByStatus = new Dictionary<string, int>
            {
                { "Pending", 10 },
                { "Processing", 20 },
                { "Shipped", 30 },
                { "Delivered", 35 },
                { "Cancelled", 5 }
            }
        };

        _mockOrderService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Order>());

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
