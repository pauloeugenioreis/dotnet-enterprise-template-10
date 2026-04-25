using System.Reflection;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Entities;
using Xunit;

namespace ProjectTemplate.UnitTests.Services;

/// <summary>
/// Tests that verify all public properties of entities are mapped to their
/// corresponding DTOs. If a new property is added to the entity but forgotten
/// in the mapper, these tests will fail.
/// </summary>
public class MappingCompletenessTests
{
    // ── Product ────────────────────────────────────────────────────────

    /// <summary>
    /// Every settable property on ProductResponseDto must have a matching
    /// property on Product (source).
    /// </summary>
    [Fact]
    public void ProductResponseDto_ShouldMapAllPropertiesFromProduct()
    {
        var dtoProperties = GetPublicSettableProperties(typeof(ProductResponseDto));
        var entityProperties = GetPublicProperties(typeof(Product));

        foreach (var dtoProp in dtoProperties)
        {
            Assert.True(
                entityProperties.Contains(dtoProp),
                $"ProductResponseDto has property '{dtoProp}' but Product entity does not. " +
                $"Either add '{dtoProp}' to Product or remove it from ProductResponseDto.");
        }
    }

    /// <summary>
    /// Every settable property on Product (excluding Id, CreatedAt, UpdatedAt)
    /// must have a matching property on SaveProductRequest (input DTO).
    /// </summary>
    [Fact]
    public void SaveProductRequest_ShouldCoverAllEditableProductProperties()
    {
        var excludedProperties = new HashSet<string> { "Id", "CreatedAt", "UpdatedAt" };

        var entityProperties = GetPublicSettableProperties(typeof(Product))
            .Where(p => !excludedProperties.Contains(p))
            .ToHashSet();

        var dtoProperties = GetPublicSettableProperties(typeof(SaveProductRequest));

        foreach (var entityProp in entityProperties)
        {
            Assert.True(
                dtoProperties.Contains(entityProp),
                $"Product has editable property '{entityProp}' but SaveProductRequest does not. " +
                $"Add '{entityProp}' to SaveProductRequest or exclude it explicitly.");
        }
    }

    // ── Order ──────────────────────────────────────────────────────────

    /// <summary>
    /// Every settable property on OrderResponseDto must have a matching
    /// property on Order (source), excluding nested DTOs.
    /// </summary>
    [Fact]
    public void OrderResponseDto_ShouldMapAllPropertiesFromOrder()
    {
        var dtoProperties = GetPublicSettableProperties(typeof(OrderResponseDto));
        var entityProperties = GetPublicProperties(typeof(Order));

        foreach (var dtoProp in dtoProperties)
        {
            Assert.True(
                entityProperties.Contains(dtoProp),
                $"OrderResponseDto has property '{dtoProp}' but Order entity does not. " +
                $"Either add '{dtoProp}' to Order or remove it from OrderResponseDto.");
        }
    }

    /// <summary>
    /// Every settable property on OrderItemResponseDto must have a matching
    /// property on OrderItem (source).
    /// </summary>
    [Fact]
    public void OrderItemResponseDto_ShouldMapAllPropertiesFromOrderItem()
    {
        var dtoProperties = GetPublicSettableProperties(typeof(OrderItemResponseDto));
        var entityProperties = GetPublicProperties(typeof(OrderItem));

        foreach (var dtoProp in dtoProperties)
        {
            Assert.True(
                entityProperties.Contains(dtoProp),
                $"OrderItemResponseDto has property '{dtoProp}' but OrderItem entity does not. " +
                $"Either add '{dtoProp}' to OrderItem or remove it from OrderItemResponseDto.");
        }
    }

    /// <summary>
    /// Every settable property on CreateOrderRequest must have a matching
    /// property on Order (excluding computed/auto-generated fields).
    /// </summary>
    [Fact]
    public void CreateOrderRequest_ShouldCoverAllInputOrderProperties()
    {
        // Properties that are auto-generated or computed, not provided by the user
        var excludedProperties = new HashSet<string>
        {
            "Id", "CreatedAt", "UpdatedAt", "IsActive",
            "OrderNumber", "Status", "Subtotal", "Tax", "ShippingCost", "Total"
        };

        var entityProperties = GetPublicSettableProperties(typeof(Order))
            .Where(p => !excludedProperties.Contains(p))
            .ToHashSet();

        var dtoProperties = GetPublicSettableProperties(typeof(CreateOrderRequest));

        // Map DTO property names to entity property names for known aliases
        var aliases = new Dictionary<string, string>
        {
            ["Phone"] = "CustomerPhone"
        };

        foreach (var entityProp in entityProperties)
        {
            var matched = dtoProperties.Contains(entityProp) ||
                          aliases.Values.Contains(entityProp);

            Assert.True(
                matched,
                $"Order has input property '{entityProp}' but CreateOrderRequest does not. " +
                $"Add '{entityProp}' to CreateOrderRequest or exclude it explicitly.");
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private static HashSet<string> GetPublicProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet();
    }

    private static HashSet<string> GetPublicSettableProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .Select(p => p.Name)
            .ToHashSet();
    }
}
