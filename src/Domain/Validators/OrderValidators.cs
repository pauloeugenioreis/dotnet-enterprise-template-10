using FluentValidation;
using ProjectTemplate.Domain.Dtos;

namespace ProjectTemplate.Domain.Validators;

/// <summary>
/// Validator for CreateOrderDto
/// </summary>
public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required")
            .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Product ID must be greater than 0");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than 0");
        });
    }
}

/// <summary>
/// Validator for UpdateOrderStatusDto
/// </summary>
public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDto>
{
    private static readonly string[] ValidStatuses = { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };

    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => ValidStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");
    }
}
