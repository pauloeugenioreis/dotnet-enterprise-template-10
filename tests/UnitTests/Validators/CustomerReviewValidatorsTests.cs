using FluentAssertions;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Validators;

namespace ProjectTemplate.UnitTests.Validators;

public class CustomerReviewValidatorsTests
{
    // ── CreateCustomerReviewValidator ────────────────────────────────────────

    [Fact]
    public async Task CreateCustomerReviewValidator_WithValidRequest_PassesValidation()
    {
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(ValidCreateRequest());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomerReviewValidator_WithEmptyProductName_FailsWithProductNameError()
    {
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "",
            CustomerName = "John",
            Rating = 5,
            Title = "Great"
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductName");
    }

    [Fact]
    public async Task CreateCustomerReviewValidator_WithProductNameExceeding200Chars_FailsWithLengthError()
    {
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = new string('A', 201),
            CustomerName = "John",
            Rating = 5,
            Title = "Great"
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductName");
    }

    [Fact]
    public async Task CreateCustomerReviewValidator_WithEmptyCustomerName_FailsWithCustomerNameError()
    {
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "Widget",
            CustomerName = "",
            Rating = 5,
            Title = "Great"
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerName");
    }

    [Fact]
    public async Task CreateCustomerReviewValidator_WithInvalidEmail_FailsWithEmailError()
    {
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "Widget",
            CustomerName = "John",
            CustomerEmail = "invalid-email",
            Rating = 5,
            Title = "Great"
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerEmail");
    }

    [Fact]
    public async Task CreateCustomerReviewValidator_WithEmptyEmail_PassesEmailValidation()
    {
        // Guard: When(x => !string.IsNullOrEmpty(x.CustomerEmail)) — empty email skips the rule
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "Widget",
            CustomerName = "John",
            CustomerEmail = null,
            Rating = 5,
            Title = "Great"
        });
        result.Errors.Should().NotContain(e => e.PropertyName == "CustomerEmail");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task CreateCustomerReviewValidator_WithRatingOutOfRange_FailsWithRatingError(int rating)
    {
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "Widget",
            CustomerName = "John",
            Rating = rating,
            Title = "Great"
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rating");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task CreateCustomerReviewValidator_WithRatingAtBoundaries_PassesValidation(int rating)
    {
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "Widget",
            CustomerName = "John",
            Rating = rating,
            Title = "Great"
        });
        result.Errors.Should().NotContain(e => e.PropertyName == "Rating");
    }

    [Fact]
    public async Task CreateCustomerReviewValidator_WithEmptyTitle_FailsWithTitleError()
    {
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "Widget",
            CustomerName = "John",
            Rating = 5,
            Title = ""
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task CreateCustomerReviewValidator_WithCommentExceeding5000Chars_FailsWithCommentError()
    {
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "Widget",
            CustomerName = "John",
            Rating = 5,
            Title = "Great",
            Comment = new string('X', 5001)
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Comment");
    }

    [Fact]
    public async Task CreateCustomerReviewValidator_With21Tags_FailsWithTagCountError()
    {
        var validator = new CreateCustomerReviewValidator();
        var tags = Enumerable.Range(1, 21).Select(i => $"tag{i}").ToList();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "Widget",
            CustomerName = "John",
            Rating = 5,
            Title = "Great",
            Tags = tags
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Tags");
    }

    [Fact]
    public async Task CreateCustomerReviewValidator_With20Tags_PassesTagCountValidation()
    {
        var validator = new CreateCustomerReviewValidator();
        var tags = Enumerable.Range(1, 20).Select(i => $"tag{i}").ToList();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "Widget",
            CustomerName = "John",
            Rating = 5,
            Title = "Great",
            Tags = tags
        });
        result.Errors.Should().NotContain(e => e.PropertyName == "Tags" && e.ErrorMessage.Contains("20 tags"));
    }

    [Fact]
    public async Task CreateCustomerReviewValidator_WithTagExceeding50Chars_FailsWithTagLengthError()
    {
        var validator = new CreateCustomerReviewValidator();
        var result = await validator.ValidateAsync(new CreateCustomerReviewRequest
        {
            ProductName = "Widget",
            CustomerName = "John",
            Rating = 5,
            Title = "Great",
            Tags = [new string('T', 51)]
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("50 characters"));
    }

    // ── ApproveCustomerReviewValidator ────────────────────────────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ApproveCustomerReviewValidator_WithIsApprovedValue_PassesValidation(bool isApproved)
    {
        var validator = new ApproveCustomerReviewValidator();
        var result = await validator.ValidateAsync(new ApproveCustomerReviewRequest { IsApproved = isApproved });
        result.IsValid.Should().BeTrue();
    }

    // ── Helpers ───────────────────���────────────────────────��─────────────────

    private static CreateCustomerReviewRequest ValidCreateRequest() => new()
    {
        ProductName = "Laptop",
        CustomerName = "John Doe",
        CustomerEmail = "john@example.com",
        Rating = 5,
        Title = "Excellent product",
        Comment = "Highly recommend",
        Tags = ["quality", "value"]
    };
}
