using FluentValidation;
using ProjectTemplate.Domain.Dtos;

namespace ProjectTemplate.Domain.Validators;

/// <summary>
/// Validation rules for creating products.
/// </summary>
public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(120).WithMessage("Category must not exceed 120 characters");
    }
}

/// <summary>
/// Validation rules for updating products.
/// </summary>
public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(120).WithMessage("Category must not exceed 120 characters");
    }
}

/// <summary>
/// Validation rules for toggling product status.
/// </summary>
public class UpdateProductStatusValidator : AbstractValidator<UpdateProductStatusRequest>
{
    public UpdateProductStatusValidator()
    {
        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("IsActive flag must be provided");
    }
}

/// <summary>
/// Validation rules for stock adjustments.
/// </summary>
public class UpdateProductStockValidator : AbstractValidator<UpdateProductStockRequest>
{
    public UpdateProductStockValidator()
    {
        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage("Quantity cannot be zero")
            .GreaterThanOrEqualTo(-100_000).WithMessage("Quantity reduction is too large")
            .LessThanOrEqualTo(100_000).WithMessage("Quantity increase is too large");
    }
}
