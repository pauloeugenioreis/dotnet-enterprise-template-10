using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Api.Controllers;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;
using Xunit;

namespace ProjectTemplate.UnitTests.Controllers;

/// <summary>
/// Unit tests for ProductController
/// Tests controller logic with mocked dependencies
/// </summary>
public class ProductControllerTests
{
    private readonly Mock<IService<Product>> _mockService;
    private readonly Mock<ILogger<ProductController>> _mockLogger;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mockService = new Mock<IService<Product>>();
        _mockLogger = new Mock<ILogger<ProductController>>();
        _controller = new ProductController(_mockService.Object, _mockLogger.Object);

        // Mock IUrlHelper for Create methods
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://localhost/api/v1/Product/1");
        _controller.Url = mockUrlHelper.Object;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOkResult_WhenProductsExist()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10.99m, Stock = 100, Category = "Electronics", IsActive = true },
            new Product { Id = 2, Name = "Product 2", Price = 20.99m, Stock = 50, Category = "Books", IsActive = true }
        };
        _mockService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAllAsync(null, null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var productId = 1L;
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Price = 15.99m,
            Stock = 75,
            Category = "Electronics",
            IsActive = true
        };
        _mockService.Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var result = await _controller.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999L;
        _mockService.Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var newProduct = new CreateProductRequest
        {
            Name = "New Product",
            Description = "Desc",
            Price = 25.99m,
            Stock = 30,
            Category = "Games",
            IsActive = true
        };
        var createdProduct = new Product
        {
            Id = 1,
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock,
            Category = newProduct.Category,
            IsActive = newProduct.IsActive
        };
        _mockService.Setup(s => s.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.CreateAsync(newProduct, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task UpdateAsync_WithValidDto_ReturnsNoContent()
    {
        // Arrange
        var productId = 1L;
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Old Product",
            Price = 10.99m,
            Stock = 50,
            Category = "Electronics",
            IsActive = true
        };
        var updatedProduct = new UpdateProductRequest
        {
            Name = "Updated Product",
            Description = existingProduct.Description,
            Price = 15.99m,
            Stock = 100,
            Category = "Electronics",
            IsActive = true
        };

        _mockService.Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(existingProduct);
        _mockService.Setup(s => s.UpdateAsync(productId, It.IsAny<Product>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateAsync(productId, updatedProduct, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999L;
        var product = new UpdateProductRequest
        {
            Name = "Test",
            Description = "Desc",
            Price = 10m,
            Stock = 10,
            Category = "Test",
            IsActive = true
        };
        _mockService.Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.UpdateAsync(productId, product, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidPayload_ReturnsNoContent()
    {
        // Arrange
        var productId = 1L;
        var existingProduct = new Product { Id = productId, Name = "Existing", Price = 5m, Stock = 5, Category = "Test", IsActive = true };

        _mockService.Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(existingProduct);
        _mockService.Setup(s => s.UpdateAsync(productId, It.IsAny<Product>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var dto = new UpdateProductStatusRequest { IsActive = false };

        // Act
        var result = await _controller.UpdateStatusAsync(productId, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        existingProduct.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var productId = 1L;
        var product = new Product { Id = productId, Name = "Test", Price = 10m, Stock = 10, Category = "Test", IsActive = true };
        _mockService.Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _mockService.Setup(s => s.DeleteAsync(productId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAsync(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999L;
        _mockService.Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.DeleteAsync(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
