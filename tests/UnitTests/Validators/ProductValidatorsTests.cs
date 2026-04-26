using FluentAssertions;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Validators;

namespace ProjectTemplate.UnitTests.Validators;

public class ProductValidatorsTests
{
    // ── SaveProductValidator ─────────────────────────────────────────────────

    [Fact]
    public async Task SaveProductValidator_WithValidRequest_PassesValidation()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task SaveProductValidator_WithEmptyName_FailsWithNameError()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest() with { Name = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task SaveProductValidator_WithNameExceeding200Chars_FailsWithLengthError()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest() with { Name = new string('A', 201) });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task SaveProductValidator_WithZeroPrice_FailsWithPriceError()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest() with { Price = 0 });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task SaveProductValidator_WithNegativePrice_FailsWithPriceError()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest() with { Price = -1m });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task SaveProductValidator_WithNegativeStock_FailsWithStockError()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest() with { Stock = -1 });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Stock");
    }

    [Fact]
    public async Task SaveProductValidator_WithZeroStock_PassesValidation()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest() with { Stock = 0 });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task SaveProductValidator_WithEmptyCategory_FailsWithCategoryError()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest() with { Category = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Category");
    }

    [Fact]
    public async Task SaveProductValidator_WithCategoryExceeding120Chars_FailsWithCategoryLengthError()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest() with { Category = new string('X', 121) });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Category");
    }

    [Fact]
    public async Task SaveProductValidator_WithDescriptionExceeding2000Chars_FailsWithDescriptionError()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest() with { Description = new string('D', 2001) });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task SaveProductValidator_WithNullDescription_PassesValidation()
    {
        var validator = new SaveProductValidator();
        var result = await validator.ValidateAsync(ValidSaveRequest() with { Description = null });
        result.IsValid.Should().BeTrue();
    }

    // ── CreateProductValidator ───────────────────────────────────────────────

    [Fact]
    public async Task CreateProductValidator_WithValidRequest_PassesValidation()
    {
        var validator = new CreateProductValidator();
        var result = await validator.ValidateAsync(new CreateProductRequest
        {
            Name = "Widget", Price = 9.99m, Stock = 10, Category = "Electronics", IsActive = true
        });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateProductValidator_WithEmptyName_FailsWithNameError()
    {
        var validator = new CreateProductValidator();
        var result = await validator.ValidateAsync(new CreateProductRequest
        {
            Name = "", Price = 9.99m, Stock = 10, Category = "Electronics", IsActive = true
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    // ── UpdateProductStatusValidator ─────────────────────────────────────────

    [Fact]
    public async Task UpdateProductStatusValidator_WithNullIsActive_FailsWithIsActiveError()
    {
        var validator = new UpdateProductStatusValidator();
        var result = await validator.ValidateAsync(new UpdateProductStatusRequest { IsActive = null });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IsActive");
    }

    [Fact]
    public async Task UpdateProductStatusValidator_WithTrueIsActive_PassesValidation()
    {
        var validator = new UpdateProductStatusValidator();
        var result = await validator.ValidateAsync(new UpdateProductStatusRequest { IsActive = true });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProductStatusValidator_WithFalseIsActive_PassesValidation()
    {
        var validator = new UpdateProductStatusValidator();
        var result = await validator.ValidateAsync(new UpdateProductStatusRequest { IsActive = false });
        result.IsValid.Should().BeTrue();
    }

    // ── UpdateProductStockValidator ──────────────────────────────────────────

    [Fact]
    public async Task UpdateProductStockValidator_WithZeroQuantity_FailsWithQuantityError()
    {
        var validator = new UpdateProductStockValidator();
        var result = await validator.ValidateAsync(new UpdateProductStockRequest { Quantity = 0 });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public async Task UpdateProductStockValidator_WithPositiveQuantity_PassesValidation()
    {
        var validator = new UpdateProductStockValidator();
        var result = await validator.ValidateAsync(new UpdateProductStockRequest { Quantity = 50 });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProductStockValidator_WithNegativeQuantity_PassesValidation()
    {
        var validator = new UpdateProductStockValidator();
        var result = await validator.ValidateAsync(new UpdateProductStockRequest { Quantity = -50 });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProductStockValidator_WithBoundaryValueMinus100000_PassesValidation()
    {
        var validator = new UpdateProductStockValidator();
        var result = await validator.ValidateAsync(new UpdateProductStockRequest { Quantity = -100_000 });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProductStockValidator_WithBoundaryValue100000_PassesValidation()
    {
        var validator = new UpdateProductStockValidator();
        var result = await validator.ValidateAsync(new UpdateProductStockRequest { Quantity = 100_000 });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProductStockValidator_WithQuantityBelowMinus100000_FailsWithTooLargeError()
    {
        var validator = new UpdateProductStockValidator();
        var result = await validator.ValidateAsync(new UpdateProductStockRequest { Quantity = -100_001 });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public async Task UpdateProductStockValidator_WithQuantityAbove100000_FailsWithTooLargeError()
    {
        var validator = new UpdateProductStockValidator();
        var result = await validator.ValidateAsync(new UpdateProductStockRequest { Quantity = 100_001 });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static SaveProductRequest ValidSaveRequest() => new()
    {
        Name = "Widget",
        Description = "A test product",
        Price = 9.99m,
        Stock = 10,
        Category = "Electronics",
        IsActive = true
    };
}
