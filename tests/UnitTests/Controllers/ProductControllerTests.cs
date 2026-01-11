using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectTemplate.Api.Controllers;
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
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithListOfProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10.99m, Stock = 100, Category = "Electronics", IsActive = true },
            new Product { Id = 2, Name = "Product 2", Price = 20.99m, Stock = 50, Category = "Books", IsActive = true }
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        returnedProducts.Should().HaveCount(2);
        returnedProducts.Should().BeEquivalentTo(products);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResult_WithProduct()
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
        _mockService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeAssignableTo<Product>().Subject;
        returnedProduct.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999L;
        _mockService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidProduct_ReturnsCreatedAtAction()
    {
        // Arrange
        var newProduct = new Product 
        { 
            Name = "New Product", 
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
        _mockService.Setup(s => s.AddAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(newProduct);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ProductController.GetById));
        createdResult.RouteValues!["id"].Should().Be(createdProduct.Id);
        var returnedProduct = createdResult.Value.Should().BeAssignableTo<Product>().Subject;
        returnedProduct.Should().BeEquivalentTo(createdProduct);
    }

    [Fact]
    public async Task Update_WithValidIdAndProduct_ReturnsOkResult()
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
        var updatedProduct = new Product 
        { 
            Id = productId, 
            Name = "Updated Product", 
            Price = 15.99m, 
            Stock = 100,
            Category = "Electronics",
            IsActive = true 
        };

        _mockService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
        _mockService.Setup(s => s.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.Update(productId, updatedProduct);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeAssignableTo<Product>().Subject;
        returnedProduct.Should().BeEquivalentTo(updatedProduct);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999L;
        var product = new Product { Id = productId, Name = "Test", Price = 10m, Stock = 10, Category = "Test", IsActive = true };
        _mockService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.Update(productId, product);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        var productId = 1L;
        var product = new Product { Id = 2L, Name = "Test", Price = 10m, Stock = 10, Category = "Test", IsActive = true };

        // Act
        var result = await _controller.Update(productId, product);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var productId = 1L;
        var product = new Product { Id = productId, Name = "Test", Price = 10m, Stock = 10, Category = "Test", IsActive = true };
        _mockService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync(product);
        _mockService.Setup(s => s.DeleteAsync(productId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999L;
        _mockService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenDeleteFails_ReturnsInternalServerError()
    {
        // Arrange
        var productId = 1L;
        var product = new Product { Id = productId, Name = "Test", Price = 10m, Stock = 10, Category = "Test", IsActive = true };
        _mockService.Setup(s => s.GetByIdAsync(productId)).ReturnsAsync(product);
        _mockService.Setup(s => s.DeleteAsync(productId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }
}
