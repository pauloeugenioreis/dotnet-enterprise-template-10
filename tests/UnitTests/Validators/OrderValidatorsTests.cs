using FluentAssertions;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Validators;

namespace ProjectTemplate.UnitTests.Validators;

public class OrderValidatorsTests
{
    // ── CreateOrderValidator ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderValidator_WithValidRequest_PassesValidation()
    {
        var validator = new CreateOrderValidator();
        var result = await validator.ValidateAsync(ValidCreateRequest());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateOrderValidator_WithEmptyCustomerName_FailsWithNameError()
    {
        var validator = new CreateOrderValidator();
        var result = await validator.ValidateAsync(ValidCreateRequest() with { CustomerName = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerName");
    }

    [Fact]
    public async Task CreateOrderValidator_WithCustomerNameExceeding200Chars_FailsWithLengthError()
    {
        var validator = new CreateOrderValidator();
        var result = await validator.ValidateAsync(ValidCreateRequest() with { CustomerName = new string('A', 201) });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerName");
    }

    [Fact]
    public async Task CreateOrderValidator_WithInvalidEmail_FailsWithEmailError()
    {
        var validator = new CreateOrderValidator();
        var result = await validator.ValidateAsync(ValidCreateRequest() with { CustomerEmail = "not-an-email" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerEmail");
    }

    [Fact]
    public async Task CreateOrderValidator_WithEmptyEmail_FailsWithEmailError()
    {
        var validator = new CreateOrderValidator();
        var result = await validator.ValidateAsync(ValidCreateRequest() with { CustomerEmail = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerEmail");
    }

    [Fact]
    public async Task CreateOrderValidator_WithShippingAddressExceeding500Chars_FailsWithAddressError()
    {
        var validator = new CreateOrderValidator();
        var result = await validator.ValidateAsync(ValidCreateRequest() with { ShippingAddress = new string('S', 501) });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ShippingAddress");
    }

    [Fact]
    public async Task CreateOrderValidator_WithEmptyItems_FailsWithItemsError()
    {
        var validator = new CreateOrderValidator();
        var result = await validator.ValidateAsync(ValidCreateRequest() with { Items = [] });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Fact]
    public async Task CreateOrderValidator_WithItemProductIdZero_FailsWithProductIdError()
    {
        var validator = new CreateOrderValidator();
        var items = new List<OrderItemDto> { new(0, 1, 10m) };
        var result = await validator.ValidateAsync(ValidCreateRequest() with { Items = items });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ProductId"));
    }

    [Fact]
    public async Task CreateOrderValidator_WithItemQuantityZero_FailsWithQuantityError()
    {
        var validator = new CreateOrderValidator();
        var items = new List<OrderItemDto> { new(1, 0, 10m) };
        var result = await validator.ValidateAsync(ValidCreateRequest() with { Items = items });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Quantity"));
    }

    [Fact]
    public async Task CreateOrderValidator_WithItemQuantityAtBoundary1000_PassesValidation()
    {
        var validator = new CreateOrderValidator();
        var items = new List<OrderItemDto> { new(1, 1000, 10m) };
        var result = await validator.ValidateAsync(ValidCreateRequest() with { Items = items });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateOrderValidator_WithItemQuantityAbove1000_FailsWithQuantityLimitError()
    {
        var validator = new CreateOrderValidator();
        var items = new List<OrderItemDto> { new(1, 1001, 10m) };
        var result = await validator.ValidateAsync(ValidCreateRequest() with { Items = items });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Quantity"));
    }

    [Fact]
    public async Task CreateOrderValidator_WithItemUnitPriceZero_FailsWithPriceError()
    {
        var validator = new CreateOrderValidator();
        var items = new List<OrderItemDto> { new(1, 1, 0m) };
        var result = await validator.ValidateAsync(ValidCreateRequest() with { Items = items });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("UnitPrice"));
    }

    [Fact]
    public async Task CreateOrderValidator_WithMultipleInvalidItems_ReportsErrorsWithIndexedPropertyNames()
    {
        var validator = new CreateOrderValidator();
        var items = new List<OrderItemDto> { new(0, 0, 0m), new(1, 1, 10m) };
        var result = await validator.ValidateAsync(ValidCreateRequest() with { Items = items });
        result.IsValid.Should().BeFalse();
        // Errors should reference Items[0] — the invalid item
        result.Errors.Should().Contain(e => e.PropertyName.Contains("[0]"));
    }

    // ── UpdateOrderStatusValidator ────────────────────────────────────────────

    [Fact]
    public async Task UpdateOrderStatusValidator_WithPendingStatus_PassesValidation()
    {
        var validator = new UpdateOrderStatusValidator();
        var result = await validator.ValidateAsync(new UpdateOrderStatusDto("Pending", null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOrderStatusValidator_WithStatusLowercase_PassesValidation()
    {
        var validator = new UpdateOrderStatusValidator();
        var result = await validator.ValidateAsync(new UpdateOrderStatusDto("shipped", null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOrderStatusValidator_WithStatusAllUppercase_PassesValidation()
    {
        var validator = new UpdateOrderStatusValidator();
        var result = await validator.ValidateAsync(new UpdateOrderStatusDto("PENDING", null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOrderStatusValidator_WithEmptyStatus_FailsWithStatusError()
    {
        var validator = new UpdateOrderStatusValidator();
        var result = await validator.ValidateAsync(new UpdateOrderStatusDto("", null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }

    [Fact]
    public async Task UpdateOrderStatusValidator_WithInvalidStatus_FailsWithAllowedStatusesError()
    {
        var validator = new UpdateOrderStatusValidator();
        var result = await validator.ValidateAsync(new UpdateOrderStatusDto("Dispatched", null));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status" && e.ErrorMessage.Contains("Status must be one of"));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static CreateOrderRequest ValidCreateRequest() => new()
    {
        CustomerName = "João Silva",
        CustomerEmail = "joao@example.com",
        ShippingAddress = "Rua das Flores, 123",
        Items = [new(1, 2, 29.99m)]
    };
}
