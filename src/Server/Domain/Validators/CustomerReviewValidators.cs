using FluentValidation;
using ProjectTemplate.Shared.Models;

namespace ProjectTemplate.Domain.Validators;

/// <summary>
/// Validation rules for creating customer reviews.
/// </summary>
public class CreateCustomerReviewValidator : AbstractValidator<CreateCustomerReviewRequest>
{
    public CreateCustomerReviewValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(150).WithMessage("Customer name must not exceed 150 characters");

        RuleFor(x => x.CustomerEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.CustomerEmail))
            .WithMessage("Invalid email address")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Comment)
            .MaximumLength(5000).WithMessage("Comment must not exceed 5000 characters");

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 20).WithMessage("Maximum of 20 tags allowed");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag cannot be empty")
            .MaximumLength(50).WithMessage("Each tag must not exceed 50 characters");
    }
}

/// <summary>
/// Validation rules for updating customer reviews.
/// </summary>
public class UpdateCustomerReviewValidator : AbstractValidator<UpdateCustomerReviewRequest>
{
    public UpdateCustomerReviewValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(150).WithMessage("Customer name must not exceed 150 characters");

        RuleFor(x => x.CustomerEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.CustomerEmail))
            .WithMessage("Invalid email address")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Comment)
            .MaximumLength(5000).WithMessage("Comment must not exceed 5000 characters");

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 20).WithMessage("Maximum of 20 tags allowed");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag cannot be empty")
            .MaximumLength(50).WithMessage("Each tag must not exceed 50 characters");
    }
}

/// <summary>
/// Validation rules for approving/rejecting customer reviews.
/// </summary>
public class ApproveCustomerReviewValidator : AbstractValidator<ApproveCustomerReviewRequest>
{
    public ApproveCustomerReviewValidator()
    {
        RuleFor(x => x.IsApproved)
            .NotNull().WithMessage("Approval status is required");
    }
}
