using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ProjectTemplate.Application.Services;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepo = new();
    private readonly Mock<IProductRepository> _mockProductRepo = new();
    private readonly Mock<ILogger<OrderService>> _mockLogger = new();
    private readonly OrderService _service;

    private const decimal TaxRate = 0.10m;
    private const decimal FreeShippingThreshold = 100m;
    private const decimal DefaultShippingCost = 15m;

    public OrderServiceTests()
    {
        var appSettings = Options.Create(new AppSettings
        {
            EnvironmentName = "Testing",
            Infrastructure = new InfrastructureSettings
            {
                Order = new OrderSettings
                {
                    TaxRate = TaxRate,
                    FreeShippingThreshold = FreeShippingThreshold,
                    DefaultShippingCost = DefaultShippingCost
                }
            }
        });

        // Make the transaction delegate actually execute the passed function
        _mockOrderRepo
            .Setup(r => r.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<OrderResponseDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<OrderResponseDto>>, CancellationToken>(
                (fn, ct) => fn(ct));

        _service = new OrderService(
            _mockOrderRepo.Object,
            _mockProductRepo.Object,
            _mockLogger.Object,
            appSettings);
    }

    // ── CreateOrderAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_WithEmptyItems_ThrowsValidationException()
    {
        var request = CreateOrderRequest(items: new List<OrderItemDto>());

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*at least one item*");
    }

    [Fact]
    public async Task CreateOrderAsync_WithNullItems_ThrowsValidationException()
    {
        var request = CreateOrderRequest() with { Items = null! };

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateOrderAsync_WithNonExistentProduct_ThrowsNotFoundException()
    {
        var request = CreateOrderRequest([new(99, 1, 10m)]);
        _mockProductRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    [Fact]
    public async Task CreateOrderAsync_WithInactiveProduct_ThrowsBusinessException()
    {
        var product = CreateProduct(1, isActive: false);
        var request = CreateOrderRequest([new(1, 1, 10m)]);
        _mockProductRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<BusinessException>().WithMessage("*not available*");
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_ThrowsBusinessException()
    {
        var product = CreateProduct(1, stock: 2);
        var request = CreateOrderRequest([new(1, 5, 10m)]);
        _mockProductRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<BusinessException>().WithMessage("*Insufficient stock*");
    }

    [Fact]
    public async Task CreateOrderAsync_WithSubtotalBelowThreshold_AppliesDefaultShippingCost()
    {
        // subtotal = 1 item * $50 = $50 → below $100 threshold → shipping = $15
        var product = CreateProduct(1, price: 50m, stock: 10);
        var request = CreateOrderRequest([new(1, 1, 50m)]);
        SetupSuccessfulCreate(product, request);

        var result = await _service.CreateOrderAsync(request);

        result.ShippingCost.Should().Be(DefaultShippingCost);
    }

    [Fact]
    public async Task CreateOrderAsync_WithSubtotalAboveThreshold_SetsShippingCostToZero()
    {
        // subtotal = 1 item * $150 = $150 → above $100 threshold → shipping = $0
        var product = CreateProduct(1, price: 150m, stock: 10);
        var request = CreateOrderRequest([new(1, 1, 150m)]);
        SetupSuccessfulCreate(product, request);

        var result = await _service.CreateOrderAsync(request);

        result.ShippingCost.Should().Be(0m);
    }

    [Fact]
    public async Task CreateOrderAsync_CalculatesTaxAsSubtotalTimesRate()
    {
        // subtotal = $100 → tax = $100 * 0.10 = $10
        var product = CreateProduct(1, price: 100m, stock: 10);
        var request = CreateOrderRequest([new(1, 1, 100m)]);
        SetupSuccessfulCreate(product, request);

        var result = await _service.CreateOrderAsync(request);

        result.Tax.Should().Be(100m * TaxRate);
    }

    [Fact]
    public async Task CreateOrderAsync_CalculatesTotalCorrectly()
    {
        // subtotal=$100, tax=$10, shipping=$0 (above threshold) → total=$110
        var product = CreateProduct(1, price: 100m, stock: 10);
        var request = CreateOrderRequest([new(1, 1, 100m)]);
        SetupSuccessfulCreate(product, request);

        var result = await _service.CreateOrderAsync(request);

        result.Total.Should().Be(result.Subtotal + result.Tax + result.ShippingCost);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidItems_DeductsStockFromProduct()
    {
        var product = CreateProduct(1, stock: 10);
        var request = CreateOrderRequest([new(1, 3, 20m)]);
        SetupSuccessfulCreate(product, request);

        await _service.CreateOrderAsync(request);

        _mockProductRepo.Verify(r => r.UpdateAsync(
            It.Is<Product>(p => p.Stock == 7),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_CallsAddAndSaveChanges()
    {
        var product = CreateProduct(1, stock: 10);
        var request = CreateOrderRequest([new(1, 1, 50m)]);
        SetupSuccessfulCreate(product, request);

        await _service.CreateOrderAsync(request);

        _mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_GeneratesOrderNumberWithOrdPrefix()
    {
        var product = CreateProduct(1, stock: 10);
        var request = CreateOrderRequest([new(1, 1, 50m)]);

        Order? capturedOrder = null;
        _mockProductRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _mockOrderRepo
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => capturedOrder = o)
            .ReturnsAsync((Order o, CancellationToken _) => o);

        await _service.CreateOrderAsync(request);

        capturedOrder!.OrderNumber.Should().StartWith("ORD-");
    }

    // ── UpdateOrderStatusAsync ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateOrderStatusAsync_WithNonExistentOrder_ThrowsNotFoundException()
    {
        _mockOrderRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var act = () => _service.UpdateOrderStatusAsync(99, new UpdateOrderStatusDto("Shipped", null));

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithInvalidStatus_ThrowsValidationException()
    {
        var order = CreateOrder(1);
        _mockOrderRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var act = () => _service.UpdateOrderStatusAsync(1, new UpdateOrderStatusDto("Dispatched", null));

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*Status must be one of*");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithValidStatusLowercase_NormalizesAndSucceeds()
    {
        var order = CreateOrder(1);
        _mockOrderRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _service.UpdateOrderStatusAsync(1, new UpdateOrderStatusDto("shipped", null));

        result.Status.Should().Be("Shipped");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithReason_AppendsNoteToOrderNotes()
    {
        var order = CreateOrder(1);
        order.Notes = "Initial notes";
        _mockOrderRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        Order? capturedOrder = null;
        _mockOrderRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => capturedOrder = o)
            .Returns(Task.CompletedTask);

        await _service.UpdateOrderStatusAsync(1, new UpdateOrderStatusDto("Shipped", "In transit"));

        capturedOrder!.Notes.Should().Contain("In transit");
        capturedOrder.Notes.Should().Contain("Initial notes");
    }

    // ── GetOrderDetailsAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetOrderDetailsAsync_WithExistingId_ReturnsMappedDto()
    {
        var order = CreateOrder(1);
        _mockOrderRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _service.GetOrderDetailsAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.CustomerEmail.Should().Be("customer@example.com");
    }

    [Fact]
    public async Task GetOrderDetailsAsync_WithNonExistingId_ReturnsNull()
    {
        _mockOrderRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var result = await _service.GetOrderDetailsAsync(99);

        result.Should().BeNull();
    }

    // ── GetOrdersByCustomerAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetOrdersByCustomerAsync_ReturnsAllMappedOrders()
    {
        var orders = new List<Order> { CreateOrder(1), CreateOrder(2) };
        _mockOrderRepo.Setup(r => r.GetByCustomerEmailAsync("customer@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var result = await _service.GetOrdersByCustomerAsync("customer@example.com");

        result.Should().HaveCount(2);
    }

    // ── GetOrdersByStatusAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetOrdersByStatusAsync_WithValidStatus_DelegatesToRepository()
    {
        _mockOrderRepo.Setup(r => r.GetByStatusAsync("Pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { CreateOrder(1) });

        var result = await _service.GetOrdersByStatusAsync("Pending");

        result.Should().HaveCount(1);
        _mockOrderRepo.Verify(r => r.GetByStatusAsync("Pending", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrdersByStatusAsync_WithInvalidStatus_ThrowsValidationException()
    {
        var act = () => _service.GetOrdersByStatusAsync("Dispatched");

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*Status must be one of*");
    }

    // ── CalculateOrderTotalAsync ─────────────────────────────────────────────

    [Fact]
    public async Task CalculateOrderTotalAsync_WithExistingOrder_ReturnsTotal()
    {
        var order = CreateOrder(1);
        order.Total = 123.45m;
        _mockOrderRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _service.CalculateOrderTotalAsync(1);

        result.Should().Be(123.45m);
    }

    [Fact]
    public async Task CalculateOrderTotalAsync_WithNonExistingOrder_ThrowsNotFoundException()
    {
        _mockOrderRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var act = () => _service.CalculateOrderTotalAsync(99);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    // ── GetStatisticsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetStatisticsAsync_DelegatesToRepository()
    {
        var stats = new OrderStatisticsDto();
        _mockOrderRepo.Setup(r => r.GetStatisticsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stats);

        var result = await _service.GetStatisticsAsync();

        result.Should().BeSameAs(stats);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static CreateOrderRequest CreateOrderRequest(List<OrderItemDto>? items = null) => new()
    {
        CustomerName = "João Silva",
        CustomerEmail = "customer@example.com",
        ShippingAddress = "Rua das Flores, 123",
        Items = items ?? [new(1, 1, 50m)]
    };

    private static Product CreateProduct(long id, decimal price = 50m, int stock = 10, bool isActive = true) => new()
    {
        Id = id,
        Name = $"Product {id}",
        Price = price,
        Stock = stock,
        Category = "General",
        IsActive = isActive
    };

    private static Order CreateOrder(long id) => new()
    {
        Id = id,
        OrderNumber = $"ORD-20260101-{id:D8}",
        CustomerName = "João Silva",
        CustomerEmail = "customer@example.com",
        ShippingAddress = "Rua das Flores, 123",
        Status = OrderStatus.Pending,
        Items = new List<OrderItem>(),
        Subtotal = 50m,
        Tax = 5m,
        ShippingCost = 15m,
        Total = 70m
    };

    private void SetupSuccessfulCreate(Product product, CreateOrderRequest request)
    {
        _mockProductRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mockOrderRepo
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken _) => o);
    }
}
