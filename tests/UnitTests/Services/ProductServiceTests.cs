using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Application.Services;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepo = new();
    private readonly Mock<ILogger<ProductService>> _mockLogger = new();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_mockRepo.Object, _mockLogger.Object);
    }

    // ── GetProductByIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetProductByIdAsync_WithExistingId_ReturnsMappedDto()
    {
        var product = CreateProduct(1);
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _service.GetProductByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Widget");
        result.Price.Should().Be(9.99m);
        result.Stock.Should().Be(50);
        result.Category.Should().Be("Electronics");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductByIdAsync_WithNonExistingId_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var result = await _service.GetProductByIdAsync(99);

        result.Should().BeNull();
    }

    // ── GetAllProductsAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAllProductsAsync_WithNoFilters_ReturnsMappedDtoList()
    {
        var products = new List<Product> { CreateProduct(1), CreateProduct(2, "Gadget") };
        _mockRepo.Setup(r => r.GetByFilterAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products.AsEnumerable(), 2));

        var (items, total) = await _service.GetAllProductsAsync();

        items.Should().HaveCount(2);
        total.Should().Be(2);
    }

    [Fact]
    public async Task GetAllProductsAsync_WithSearchTerm_PassesFilterToRepository()
    {
        _mockRepo.Setup(r => r.GetByFilterAsync("Widget", null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<Product>(), 0));

        await _service.GetAllProductsAsync(searchTerm: "Widget");

        _mockRepo.Verify(r => r.GetByFilterAsync("Widget", null, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_WithPagination_PassesPageToRepository()
    {
        _mockRepo.Setup(r => r.GetByFilterAsync(null, null, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<Product>(), 0));

        await _service.GetAllProductsAsync(page: 2, pageSize: 10);

        _mockRepo.Verify(r => r.GetByFilterAsync(null, null, 2, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── CreateProductAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateProductAsync_WithValidRequest_CallsAddAndSaveChanges()
    {
        var request = new CreateProductRequest { Name = "Widget", Price = 9.99m, Stock = 50, Category = "Electronics", IsActive = true };
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => { p.Id = 1; return p; });

        await _service.CreateProductAsync(request);

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_MapsAllFieldsCorrectly()
    {
        var request = new CreateProductRequest
        {
            Name = "Widget",
            Description = "A fine widget",
            Price = 9.99m,
            Stock = 50,
            Category = "Electronics",
            IsActive = false
        };
        Product? captured = null;
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync((Product p, CancellationToken _) => p);

        await _service.CreateProductAsync(request);

        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Widget");
        captured.Description.Should().Be("A fine widget");
        captured.Price.Should().Be(9.99m);
        captured.Stock.Should().Be(50);
        captured.Category.Should().Be("Electronics");
        captured.IsActive.Should().BeFalse();
    }

    // ── UpdateProductAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProductAsync_WithExistingId_CallsUpdateAndSaveChanges()
    {
        var product = CreateProduct(1);
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await _service.UpdateProductAsync(1, new UpdateProductRequest { Name = "Updated", Price = 19.99m, Stock = 10, Category = "Tools", IsActive = true });

        _mockRepo.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var act = () => _service.UpdateProductAsync(99, new UpdateProductRequest { Name = "X", Price = 1, Stock = 1, Category = "X", IsActive = true });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    // ── UpdateProductStatusAsync ─────────────────────────────────────────────

    [Fact]
    public async Task UpdateProductStatusAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var act = () => _service.UpdateProductStatusAsync(99, new UpdateProductStatusRequest { IsActive = true });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    [Fact]
    public async Task UpdateProductStatusAsync_WithNullIsActive_KeepsExistingStatus()
    {
        var product = CreateProduct(1);
        product.IsActive = true;
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await _service.UpdateProductStatusAsync(1, new UpdateProductStatusRequest { IsActive = null });

        _mockRepo.Verify(r => r.UpdateAsync(
            It.Is<Product>(p => p.IsActive == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductStatusAsync_WithFalseIsActive_SetsProductInactive()
    {
        var product = CreateProduct(1);
        product.IsActive = true;
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await _service.UpdateProductStatusAsync(1, new UpdateProductStatusRequest { IsActive = false });

        _mockRepo.Verify(r => r.UpdateAsync(
            It.Is<Product>(p => p.IsActive == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── UpdateProductStockAsync ──────────────────────────────────────────────

    [Fact]
    public async Task UpdateProductStockAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var act = () => _service.UpdateProductStockAsync(99, new UpdateProductStockRequest { Quantity = 10 });

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*99*");
    }

    [Fact]
    public async Task UpdateProductStockAsync_WithQuantityThatCausesNegativeStock_ThrowsBusinessException()
    {
        var product = CreateProduct(1, stock: 5);
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var act = () => _service.UpdateProductStockAsync(1, new UpdateProductStockRequest { Quantity = -10 });

        await act.Should().ThrowAsync<BusinessException>().WithMessage("*Insufficient stock*");
    }

    [Fact]
    public async Task UpdateProductStockAsync_WithPositiveQuantity_IncreasesStock()
    {
        var product = CreateProduct(1, stock: 20);
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _service.UpdateProductStockAsync(1, new UpdateProductStockRequest { Quantity = 5 });

        result.Stock.Should().Be(25);
        _mockRepo.Verify(r => r.UpdateAsync(
            It.Is<Product>(p => p.Stock == 25), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductStockAsync_WithNegativeQuantity_DecreasesStock()
    {
        var product = CreateProduct(1, stock: 20);
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _service.UpdateProductStockAsync(1, new UpdateProductStockRequest { Quantity = -5 });

        result.Stock.Should().Be(15);
    }

    [Fact]
    public async Task UpdateProductStockAsync_WithExactStockDepletion_Succeeds()
    {
        var product = CreateProduct(1, stock: 10);
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _service.UpdateProductStockAsync(1, new UpdateProductStockRequest { Quantity = -10 });

        result.Stock.Should().Be(0);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Product CreateProduct(long id, string name = "Widget", int stock = 50) => new()
    {
        Id = id,
        Name = name,
        Description = "A test product",
        Price = 9.99m,
        Stock = stock,
        Category = "Electronics",
        IsActive = true
    };
}
