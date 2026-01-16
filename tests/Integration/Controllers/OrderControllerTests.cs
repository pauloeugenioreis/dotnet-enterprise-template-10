using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Entities;
using Xunit;

namespace ProjectTemplate.Integration.Tests.Controllers;

/// <summary>
/// Integration tests for OrderController
/// </summary>
public class OrderControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;

    public OrderControllerTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Order");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_ValidOrder_ReturnsCreated()
    {
        // Arrange - Create a product first
        var product = new Product
        {
            Name = "Test Product for Order",
            Description = "Test",
            Price = 50.00m,
            Stock = 100,
            Category = "Electronics",
            IsActive = true
        };
        var productResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await productResponse.Content.ReadFromJsonAsync<Product>();

        var orderDto = new CreateOrderRequest
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            Phone = "+1234567890",
            ShippingAddress = "123 Main St, City, State 12345",
            Items = new List<OrderItemDto>
            {
                new(createdProduct!.Id, 2, 50.00m)
            },
            Notes = "Test order"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Order", orderDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdOrder = await response.Content.ReadFromJsonAsync<OrderResponseDto>();
        createdOrder.Should().NotBeNull();
        createdOrder!.CustomerName.Should().Be("John Doe");
        createdOrder.Items.Should().HaveCount(1);
        createdOrder.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByCustomer_ReturnsOrders()
    {
        // Arrange - Create order first
        var product = new Product
        {
            Name = "Product for Customer Test",
            Price = 30.00m,
            Stock = 50,
            IsActive = true
        };
        var productResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await productResponse.Content.ReadFromJsonAsync<Product>();

        var orderDto = new CreateOrderRequest
        {
            CustomerName = "Jane Smith",
            CustomerEmail = "jane@example.com",
            ShippingAddress = "456 Oak Ave",
            Items = new List<OrderItemDto> { new(createdProduct!.Id, 1, 30.00m) }
        };
        var orderCreateResponse = await _client.PostAsJsonAsync("/api/v1/Order", orderDto);
        orderCreateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var response = await _client.GetAsync("/api/v1/Order/customer/jane@example.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponseDto>>();
        orders.Should().NotBeNull();
        orders!.Should().NotBeEmpty("because we just created an order for this customer");
        orders.First().CustomerEmail.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task UpdateStatus_ValidOrder_ReturnsSuccess()
    {
        // Arrange - Create order first
        var product = new Product
        {
            Name = "Product for Status Test",
            Price = 40.00m,
            Stock = 50,
            IsActive = true
        };
        var productResponse = await _client.PostAsJsonAsync("/api/v1/Product", product);
        var createdProduct = await productResponse.Content.ReadFromJsonAsync<Product>();

        var orderDto = new CreateOrderRequest
        {
            CustomerName = "Test User",
            CustomerEmail = "test@example.com",
            ShippingAddress = "789 Elm St",
            Items = new List<OrderItemDto> { new(createdProduct!.Id, 1, 40.00m) }
        };
        var orderResponse = await _client.PostAsJsonAsync("/api/v1/Order", orderDto);
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdOrder = await orderResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        createdOrder.Should().NotBeNull();

        var statusDto = new UpdateOrderStatusDto("Processing", "Order is being processed");

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/v1/Order/{createdOrder!.Id}/status", statusDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update by getting the order
        var getResponse = await _client.GetAsync($"/api/v1/Order/{createdOrder.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedOrder = await getResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        updatedOrder!.Status.Should().Be("Processing");
    }

    [Fact]
    public async Task GetStatistics_ReturnsStatistics()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Order/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("totalOrders");
    }

    [Fact]
    public async Task ExportToExcel_WithoutFilters_ReturnsExcelFile()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Order/ExportToExcel");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
        content.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportToExcel_WithStatusFilter_ReturnsFilteredExcelFile()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Order/ExportToExcel?status=Pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
    }
}
