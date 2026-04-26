using ProjectTemplate.Shared.Models;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectTemplate.Shared.Models;
using Xunit;

namespace ProjectTemplate.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for ProductController
/// </summary>
[Collection("Integration Tests")]
public class ProductControllerTests
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;

    public ProductControllerTests(WebApplicationFactoryFixture factory)
    {
        factory.ClearEventStore();
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
        var product = new CreateProductRequest
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

        var createdProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be("Test Product");
        createdProduct.Price.Should().Be(29.99m);
    }

    [Fact]
    public async Task Create_InvalidProduct_ReturnsBadRequest()
    {
        // Arrange - empty name should trigger ValidationFilter
        var product = new CreateProductRequest
        {
            Name = "",
            Description = "Test Description",
            Price = -10m, // Invalid price
            Stock = 100,
            Category = "Electronics",
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Product", product);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Name");
        content.Should().Contain("Price");
    }

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsProduct()
    {
        // Arrange - Create a product first
        var product = new CreateProductRequest
        {
            Name = "Test Product for GetById",
            Description = "Test",
            Price = 19.99m,
            Stock = 50,
            Category = "Test",
            IsActive = true
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        // Act
        var response = await _client.GetAsync($"/api/v1/Product/{createdProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedProduct = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
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
        var product = new CreateProductRequest
        {
            Name = "Original Name",
            Description = "Original Description",
            Price = 15.00m,
            Stock = 30,
            Category = "Original",
            IsActive = true
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        var productToUpdate = new UpdateProductRequest
        {
            Name = "Updated Name",
            Description = createdProduct!.Description,
            Price = 25.00m,
            Stock = createdProduct.Stock,
            Category = createdProduct.Category,
            IsActive = createdProduct.IsActive
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/Product/{createdProduct.Id}", productToUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update by getting the product
        var getResponse = await _client.GetAsync($"/api/v1/Product/{createdProduct.Id}");
        var updatedProduct = await getResponse.Content.ReadFromJsonAsync<ProductResponseDto>();
        updatedProduct!.Name.Should().Be("Updated Name");
        updatedProduct.Price.Should().Be(25.00m);
    }

    [Fact]
    public async Task Delete_ExistingProduct_ReturnsNoContent()
    {
        // Arrange - Create a product first
        var product = new CreateProductRequest
        {
            Name = "Product to Delete",
            Description = "Will be deleted",
            Price = 10.00m,
            Stock = 10,
            Category = "Temp",
            IsActive = true
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

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

    [Fact]
    public async Task UpdateStatus_ExistingProduct_ReturnsNoContent()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            Name = "Status Product",
            Description = "Status test",
            Price = 17.50m,
            Stock = 25,
            Category = "Status",
            IsActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        var statusRequest = new UpdateProductStatusRequest
        {
            IsActive = false
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/Product/{created!.Id}/status", statusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/Product/{created.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ProductResponseDto>();
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateStock_ExistingProduct_ReturnsUpdatedProduct()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            Name = "Stock Product",
            Description = "Stock test",
            Price = 42.00m,
            Stock = 10,
            Category = "Stock",
            IsActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        var stockRequest = new UpdateProductStockRequest
        {
            Quantity = 5
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/Product/{created!.Id}/stock", stockRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        payload.Should().NotBeNull();
        payload!.Stock.Should().Be(15);
    }

    [Fact]
    public async Task Delete_NonExistentProduct_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/v1/Product/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_NonExistentProduct_ReturnsNotFound()
    {
        var payload = new UpdateProductRequest
        {
            Name = "Ghost Product",
            Price = 10m,
            Stock = 0,
            Category = "Ghost",
            IsActive = false
        };

        var response = await _client.PutAsJsonAsync("/api/v1/Product/99999", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStock_WithZeroQuantity_ReturnsBadRequest()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/Product", new CreateProductRequest
        {
            Name = "Stock Validation Product",
            Price = 5m,
            Stock = 10,
            Category = "Test",
            IsActive = true
        });
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>();

        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/Product/{created!.Id}/stock",
            new UpdateProductStockRequest { Quantity = 0 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Quantity");
    }
}
