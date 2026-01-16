using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectTemplate.Domain.Entities;
using Xunit;

namespace ProjectTemplate.Integration.Tests.Controllers;

/// <summary>
/// Integration tests for ProductController
/// </summary>
public class ProductControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;

    public ProductControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Product");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_ValidProduct_ReturnsCreated()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 29.99m,
            Stock = 100,
            Category = "Electronics",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Product", product);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdProduct = await response.Content.ReadFromJsonAsync<Product>();
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be("Test Product");
        createdProduct.Price.Should().Be(29.99m);
    }

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsProduct()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Product for GetById",
            Description = "Test",
            Price = 19.99m,
            Stock = 50,
            Category = "Test",
            IsActive = true
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<Product>();

        // Act
        var response = await _client.GetAsync($"/api/v1/Product/{createdProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedProduct = await response.Content.ReadFromJsonAsync<Product>();
        retrievedProduct.Should().NotBeNull();
        retrievedProduct!.Id.Should().Be(createdProduct.Id);
    }

    [Fact]
    public async Task GetById_NonExistentProduct_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Product/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ExistingProduct_ReturnsSuccess()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Original Name",
            Description = "Original Description",
            Price = 15.00m,
            Stock = 30,
            Category = "Original",
            IsActive = true
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<Product>();

        // Create a new instance to avoid EF Core tracking conflict
        var productToUpdate = new Product
        {
            Id = createdProduct!.Id,
            Name = "Updated Name",
            Description = createdProduct.Description,
            Price = 25.00m,
            Stock = createdProduct.Stock,
            Category = createdProduct.Category,
            IsActive = createdProduct.IsActive
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/Product/{productToUpdate.Id}", productToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update by getting the product
        var getResponse = await _client.GetAsync($"/api/v1/Product/{productToUpdate.Id}");
        var updatedProduct = await getResponse.Content.ReadFromJsonAsync<Product>();
        updatedProduct!.Name.Should().Be("Updated Name");
        updatedProduct.Price.Should().Be(25.00m);
    }

    [Fact]
    public async Task Delete_ExistingProduct_ReturnsNoContent()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Product to Delete",
            Description = "Will be deleted",
            Price = 10.00m,
            Stock = 10,
            Category = "Temp",
            IsActive = true
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<Product>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/Product/{createdProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/v1/Product/{createdProduct.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExportToExcel_WithoutFilters_ReturnsExcelFile()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Product/ExportToExcel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
        content.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportToExcel_WithFilters_ReturnsFilteredExcelFile()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Product/ExportToExcel?isActive=true&category=Electronics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
    }
}
