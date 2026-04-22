using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Api.Controllers;
using ProjectTemplate.SharedModels;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;
using Xunit;

namespace ProjectTemplate.UnitTests.Controllers;

/// <summary>
/// Unit tests for ProductController
/// Tests controller logic with mocked dependencies
/// </summary>
public class ProductControllerTests
{
    private readonly Mock<IProductService> _mockService;
    private readonly Mock<ILogger<ProductController>> _mockLogger;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mockService = new Mock<IProductService>();
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
        var products = new List<ProductResponseDto>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.99m, Stock = 100, Category = "Electronics", IsActive = true },
            new() { Id = 2, Name = "Product 2", Price = 20.99m, Stock = 50, Category = "Books", IsActive = true }
        };
        _mockService.Setup(s => s.GetAllProductsAsync(null, null, null, null, It.IsAny<CancellationToken>())).ReturnsAsync((products, products.Count));

        // Act
        var result = await _controller.GetAllAsync(null, null, null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var productId = 1L;
        var product = new ProductResponseDto
        {
            Id = productId,
            Name = "Test Product",
            Price = 15.99m,
            Stock = 75,
            Category = "Electronics",
            IsActive = true
        };
        _mockService.Setup(s => s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

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
        _mockService.Setup(s => s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((ProductResponseDto?)null);

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
        var createdProduct = new ProductResponseDto
        {
            Id = 1,
            Name = newProduct.Name,
            Price = newProduct.Price,
            Stock = newProduct.Stock,
            Category = newProduct.Category,
            IsActive = newProduct.IsActive
        };
        _mockService.Setup(s => s.CreateProductAsync(newProduct, It.IsAny<CancellationToken>())).ReturnsAsync(createdProduct);

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
        var updatedProduct = new UpdateProductRequest
        {
            Name = "Updated Product",
            Price = 15.99m,
            Stock = 100,
            Category = "Electronics",
            IsActive = true
        };

        _mockService.Setup(s => s.UpdateProductAsync(productId, updatedProduct, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateAsync(productId, updatedProduct, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ThrowsNotFoundException()
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
        _mockService.Setup(s => s.UpdateProductAsync(productId, product, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Product with ID {productId} not found"));

        // Act
        var act = () => _controller.UpdateAsync(productId, product, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidPayload_ReturnsNoContent()
    {
        // Arrange
        var productId = 1L;
        var dto = new UpdateProductStatusRequest { IsActive = false };

        _mockService.Setup(s => s.UpdateProductStatusAsync(productId, dto, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateStatusAsync(productId, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var productId = 1L;
        _mockService.Setup(s => s.DeleteAsync(productId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAsync(productId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 999L;
        _mockService.Setup(s => s.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Entity with ID {productId} not found"));

        // Act
        var act = () => _controller.DeleteAsync(productId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
