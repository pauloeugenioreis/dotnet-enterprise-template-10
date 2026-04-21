using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using ProjectTemplate.Api.Controllers;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;
using Xunit;

namespace ProjectTemplate.Integration.Tests.Controllers;

/// <summary>
/// Integration tests for AuditController to guarantee the Event Sourcing surface stays functional.
/// </summary>
public class AuditControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public AuditControllerTests(WebApplicationFactoryFixture factory)
    {
        factory.ClearEventStore();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEntityHistory_ReturnsAllRecordedEvents()
    {
        // Arrange
        var orderId = await SeedOrderLifecycleAsync(additionalUpdates: 1);

        // Act
        var response = await _client.GetAsync($"/api/v1/Audit/Order/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var events = await response.Content.ReadFromJsonAsync<List<DomainEvent>>();
        events.Should().NotBeNull();
        events!.Count.Should().BeGreaterThan(1, "create + update events must exist");
        events.Select(e => e.EventType).Should().Contain(new[] { "OrderCreatedEvent", "OrderUpdatedEvent" });
    }

    [Fact]
    public async Task TimeTravel_ReturnsStateUpToCutoffTimestamp()
    {
        // Arrange
        var product = await CreateProductAsync();
        var order = await CreateOrderAsync(product.Id);
        var cutoff = DateTime.UtcNow;

        await Task.Delay(TimeSpan.FromMilliseconds(10));
        await UpdateOrderStatusAsync(order.Id, "Processing");

        // Act
        var response = await _client.GetAsync($"/api/v1/Audit/Order/{order.Id}/at/{Uri.EscapeDataString(cutoff.ToString("O"))}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<AuditStateResponse>();
        payload.Should().NotBeNull();
        payload!.EventCount.Should().Be(1);
        payload.Events.Should().ContainSingle();
        payload.Events[0].EventType.Should().Be("OrderCreatedEvent");
    }

    [Fact]
    public async Task ReplayEvents_ReturnsOrderedTimeline()
    {
        // Arrange
        var orderId = await SeedOrderLifecycleAsync(additionalUpdates: 2);

        // Act
        var response = await _client.PostAsync($"/api/v1/Audit/Order/{orderId}/replay", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ReplayResponse>();
        payload.Should().NotBeNull();
        payload!.EventCount.Should().Be(payload.Events.Count);
        payload.Events.Should().BeInAscendingOrder(e => e.Version);
        payload.Events.Select(e => e.EventType).Should().Contain(new[] { "OrderCreatedEvent", "OrderUpdatedEvent" });
    }

    [Fact]
    public async Task GetEventsByVersion_ReturnsFilteredRange()
    {
        // Arrange
        var orderId = await SeedOrderLifecycleAsync(additionalUpdates: 2);

        // Act
        var response = await _client.GetAsync($"/api/v1/Audit/Order/{orderId}/versions/1/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<DomainEvent>>();
        events.Should().NotBeNull();
        events!.Should().ContainSingle();
        events[0].Version.Should().Be(1);
    }

    [Fact]
    public async Task GetEventsByType_ReturnsOrderEvents()
    {
        // Arrange
        await SeedOrderLifecycleAsync(additionalUpdates: 1);

        // Act
        var response = await _client.GetAsync("/api/v1/Audit/Order?page=1&pageSize=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<DomainEvent>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(e => e.AggregateType == "Order");
    }

    [Fact]
    public async Task GetEventsByUser_ReturnsOkPayload()
    {
        // Arrange
        await SeedOrderLifecycleAsync(additionalUpdates: 1);

        // Act
        var response = await _client.GetAsync("/api/v1/Audit/user/integration-user?limit=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuditUserResponse>();
        payload.Should().NotBeNull();
        payload!.UserId.Should().Be("integration-user");
        payload.EventCount.Should().BeGreaterThanOrEqualTo(0);
        payload.Events.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStatistics_ReturnsAggregatedMetrics()
    {
        // Arrange
        await SeedOrderLifecycleAsync(additionalUpdates: 1);

        // Act
        var response = await _client.GetAsync("/api/v1/Audit/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<EventStatistics>();
        payload.Should().NotBeNull();
        payload!.TotalEvents.Should().BeGreaterThan(0);
        payload.EventsByAggregateType.Should().ContainKey("Order");
    }

    // -------------------------------------------------------------------------
    // Item 2 — Product events recorded by EventSourcing (HybridRepository)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ProductCreate_GeneratesAuditEvent()
    {
        // Arrange
        var product = await CreateProductAsync();

        // Act — query audit history for this specific Product
        var response = await _client.GetAsync($"/api/v1/Audit/Product/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<DomainEvent>>();
        events.Should().NotBeNull();
        events!.Should().NotBeEmpty("a ProductCreatedEvent must be recorded when a product is created");
        events.Should().Contain(e => e.AggregateType == "Product");
        events.Should().Contain(e => e.EventType == "ProductCreatedEvent");
    }

    [Fact]
    public async Task ProductUpdate_GeneratesAuditEvent()
    {
        // Arrange
        var product = await CreateProductAsync();
        await UpdateProductAsync(product.Id);

        // Act
        var response = await _client.GetAsync($"/api/v1/Audit/Product/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<DomainEvent>>();
        events.Should().NotBeNull();
        events!.Select(e => e.EventType)
            .Should().Contain(new[] { "ProductCreatedEvent", "ProductUpdatedEvent" },
                because: "create followed by update must each produce an event");
    }

    [Fact]
    public async Task ProductStatusChange_GeneratesAuditEvent()
    {
        // Arrange
        var product = await CreateProductAsync();
        await ToggleProductStatusAsync(product.Id, isActive: false);

        // Act
        var response = await _client.GetAsync($"/api/v1/Audit/Product/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<DomainEvent>>();
        events.Should().NotBeNull();

        // The HybridRepository emits ProductUpdatedEvent for all mutations;
        // we distinguish status changes by checking the EventData payload.
        var statusChangeEvent = events!.FirstOrDefault(e =>
            e.EventType == "ProductUpdatedEvent" &&
            e.EventData?.ToString()?.Contains("IsActive", StringComparison.OrdinalIgnoreCase) == true);

        statusChangeEvent.Should().NotBeNull(
            because: "toggling IsActive must produce a ProductUpdatedEvent with 'IsActive' in the Changes payload");
    }

    [Fact]
    public async Task ProductStockAdjustment_GeneratesAuditEvent()
    {
        // Arrange
        var product = await CreateProductAsync();
        await AdjustProductStockAsync(product.Id, quantity: 10);

        // Act
        var response = await _client.GetAsync($"/api/v1/Audit/Product/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<DomainEvent>>();
        events.Should().NotBeNull();

        // The HybridRepository emits ProductUpdatedEvent for all mutations;
        // we distinguish stock changes by checking the EventData payload.
        var stockChangeEvent = events!.FirstOrDefault(e =>
            e.EventType == "ProductUpdatedEvent" &&
            e.EventData?.ToString()?.Contains("Stock", StringComparison.OrdinalIgnoreCase) == true);

        stockChangeEvent.Should().NotBeNull(
            because: "a stock adjustment must produce a ProductUpdatedEvent with 'Stock' in the Changes payload");
    }

    [Fact]
    public async Task GetEntitiesHistory_Product_ContainsOnlyProductEvents()
    {
        // Arrange — ensure at least one product event exists
        await CreateProductAsync();

        // Act — query all Product events (no specific ID)
        var response = await _client.GetAsync("/api/v1/Audit/Product?page=1&pageSize=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<DomainEvent>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(e => e.AggregateType == "Product",
            because: "filtering by entityType=Product must not leak Order events");
    }

    // -------------------------------------------------------------------------
    // Item 3 — Pagination of EventStore (GetEventsPagedAsync / GetEventsByTypeAsync)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetEntityHistory_WithPagination_ReturnsCorrectPage()
    {
        // Arrange — seed an order with 3 events (create + 2 updates)
        var orderId = await SeedOrderLifecycleAsync(additionalUpdates: 2);

        // Act — page 1, size 1 → should return only the first event
        var response = await _client.GetAsync($"/api/v1/Audit/Order/{orderId}?page=1&pageSize=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<DomainEvent>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1, "pageSize=1 must return exactly one event");
        result.Total.Should().BeGreaterThanOrEqualTo(3,
            because: "create + 2 updates = at least 3 total events");
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task GetEntityHistory_Page2_ReturnsDifferentEvent()
    {
        // Arrange — seed an order with 3 events
        var orderId = await SeedOrderLifecycleAsync(additionalUpdates: 2);

        // Act — retrieve page 1 and page 2 with pageSize=1
        var page1Response = await _client.GetAsync($"/api/v1/Audit/Order/{orderId}?page=1&pageSize=1");
        var page2Response = await _client.GetAsync($"/api/v1/Audit/Order/{orderId}?page=2&pageSize=1");

        page1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        page2Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page1 = await page1Response.Content.ReadFromJsonAsync<PagedResponse<DomainEvent>>();
        var page2 = await page2Response.Content.ReadFromJsonAsync<PagedResponse<DomainEvent>>();

        // Assert — the two pages must have different events
        page1.Should().NotBeNull();
        page2.Should().NotBeNull();
        page1!.Items.Should().HaveCount(1);
        page2!.Items.Should().HaveCount(1);
        page1.Items.First().Id.Should().NotBe(page2.Items.First().Id,
            because: "page 1 and page 2 must return different events");
    }

    [Fact]
    public async Task GetEventsByType_WithPagination_HonorsPageSizeLimit()
    {
        // Arrange — seed multiple orders so we have several Order events
        await SeedOrderLifecycleAsync(additionalUpdates: 1);
        await SeedOrderLifecycleAsync(additionalUpdates: 1);

        // Act — request only 2 events per page
        var response = await _client.GetAsync("/api/v1/Audit/Order?page=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<DomainEvent>>();
        result.Should().NotBeNull();
        result!.Items.Count().Should().BeLessThanOrEqualTo(2,
            because: "pageSize=2 must never return more than 2 items");
        result.Total.Should().BeGreaterThan(2,
            because: "two order lifecycles must produce more than 2 events in total");
    }

    [Fact]
    public async Task GetEntityHistory_NoPagination_ReturnsAllEvents()
    {
        // Arrange — seed 3 events for a single order
        var orderId = await SeedOrderLifecycleAsync(additionalUpdates: 2);

        // Act — no page/pageSize query params → returns raw list
        var response = await _client.GetAsync($"/api/v1/Audit/Order/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var events = await response.Content.ReadFromJsonAsync<List<DomainEvent>>();
        events.Should().NotBeNull();
        events!.Count.Should().BeGreaterThanOrEqualTo(3,
            because: "without pagination the full event list must be returned");
    }

    private async Task<long> SeedOrderLifecycleAsync(int additionalUpdates)
    {
        var product = await CreateProductAsync();
        var order = await CreateOrderAsync(product.Id);

        var statuses = new[] { "Processing", "Shipped", "Delivered" };
        for (var i = 0; i < additionalUpdates && i < statuses.Length; i++)
        {
            await UpdateOrderStatusAsync(order.Id, statuses[i]);
        }

        return order.Id;
    }

    private async Task<ProductResponseDto> CreateProductAsync()
    {
        var dto = new CreateProductRequest
        {
            Name = $"Audit Product {Guid.NewGuid():N}",
            Description = "Product created for audit integration tests",
            Price = 99.90m,
            Stock = 50,
            Category = "audit",
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Product", dto);
        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<ProductResponseDto>();
        return product ?? throw new InvalidOperationException("Product creation failed");
    }

    private async Task<OrderResponseDto> CreateOrderAsync(long productId)
    {
        var dto = new CreateOrderRequest
        {
            CustomerName = "Audit Customer",
            CustomerEmail = $"audit+{Guid.NewGuid():N}@example.com",
            Phone = "+5511999999999",
            ShippingAddress = "Rua Audit, 123 - São Paulo",
            Notes = "Integration test order",
            Items = new List<OrderItemDto> { new(productId, 1, 99.90m) }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Order", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await response.Content.ReadFromJsonAsync<OrderResponseDto>();
        return order ?? throw new InvalidOperationException("Order creation failed");
    }

    private async Task UpdateProductAsync(long productId)
    {
        var dto = new UpdateProductRequest
        {
            Name = $"Updated Product {Guid.NewGuid():N}",
            Description = "Updated by audit integration test",
            Price = 149.90m,
            Stock = 75,
            Category = "audit-updated",
            IsActive = true
        };
        var response = await _client.PutAsJsonAsync($"/api/v1/Product/{productId}", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            $"product update should succeed but got: {await response.Content.ReadAsStringAsync()}");
    }

    private async Task ToggleProductStatusAsync(long productId, bool isActive)
    {
        var dto = new UpdateProductStatusRequest { IsActive = isActive };
        var response = await _client.PatchAsJsonAsync($"/api/v1/Product/{productId}/status", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            $"product status toggle should succeed but got: {await response.Content.ReadAsStringAsync()}");
    }

    private async Task AdjustProductStockAsync(long productId, int quantity)
    {
        var dto = new UpdateProductStockRequest { Quantity = quantity };
        var response = await _client.PatchAsJsonAsync($"/api/v1/Product/{productId}/stock", dto);
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"stock adjustment should succeed but got: {await response.Content.ReadAsStringAsync()}");
    }

    private async Task UpdateOrderStatusAsync(long orderId, string status)
    {
        var dto = new UpdateOrderStatusDto(status, $"Status changed to {status} during integration tests");
        var response = await _client.PatchAsJsonAsync($"/api/v1/Order/{orderId}/status", dto);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private sealed record AuditStateResponse(
        string EntityType,
        string EntityId,
        DateTime Timestamp,
        int EventCount,
        List<AuditStateEvent> Events);

    private sealed record AuditStateEvent(
        string EventType,
        int Version,
        DateTime OccurredOn,
        string? UserId);

    private sealed record ReplayResponse(
        string EntityType,
        string EntityId,
        int EventCount,
        string ReconstructedState,
        List<ReplayEvent> Events);

    private sealed record ReplayEvent(
        string EventType,
        int Version,
        DateTime OccurredOn);

    private sealed record AuditUserResponse(
        string UserId,
        int EventCount,
        List<JsonElement> Events);
}
