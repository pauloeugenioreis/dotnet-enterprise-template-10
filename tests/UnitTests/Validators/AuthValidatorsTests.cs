using FluentAssertions;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Validators;

namespace ProjectTemplate.UnitTests.Validators;

public class AuthValidatorsTests
{
    // ── RegisterDtoValidator ─────────────────────────────────────────────────

    [Fact]
    public async Task RegisterDtoValidator_WithValidDto_PassesValidation()
    {
        var validator = new RegisterDtoValidator();
        var result = await validator.ValidateAsync(new RegisterDto { Email = "user@example.com", Password = "Password1!" });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterDtoValidator_WithEmptyEmail_FailsWithEmailError()
    {
        var validator = new RegisterDtoValidator();
        var result = await validator.ValidateAsync(new RegisterDto { Email = "", Password = "Password1!" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task RegisterDtoValidator_WithInvalidEmail_FailsWithEmailError()
    {
        var validator = new RegisterDtoValidator();
        var result = await validator.ValidateAsync(new RegisterDto { Email = "not-an-email", Password = "Password1!" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task RegisterDtoValidator_WithPasswordShorterThan8_FailsWithLengthError()
    {
        var validator = new RegisterDtoValidator();
        var result = await validator.ValidateAsync(new RegisterDto { Email = "user@example.com", Password = "Abc1!" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("8 characters"));
    }

    [Fact]
    public async Task RegisterDtoValidator_WithPasswordMissingUppercase_FailsWithUppercaseError()
    {
        var validator = new RegisterDtoValidator();
        // All lowercase + digit + special char — missing uppercase
        var result = await validator.ValidateAsync(new RegisterDto { Email = "user@example.com", Password = "password1!" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("uppercase"));
    }

    [Fact]
    public async Task RegisterDtoValidator_WithPasswordMissingLowercase_FailsWithLowercaseError()
    {
        var validator = new RegisterDtoValidator();
        // All uppercase + digit + special char — missing lowercase
        var result = await validator.ValidateAsync(new RegisterDto { Email = "user@example.com", Password = "PASSWORD1!" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("lowercase"));
    }

    [Fact]
    public async Task RegisterDtoValidator_WithPasswordMissingDigit_FailsWithDigitError()
    {
        var validator = new RegisterDtoValidator();
        // Letters + special char — missing digit
        var result = await validator.ValidateAsync(new RegisterDto { Email = "user@example.com", Password = "Password!" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("digit"));
    }

    [Fact]
    public async Task RegisterDtoValidator_WithPasswordMissingSpecialChar_FailsWithSpecialCharError()
    {
        var validator = new RegisterDtoValidator();
        // Letters + digits — missing special char
        var result = await validator.ValidateAsync(new RegisterDto { Email = "user@example.com", Password = "Password1" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("special character"));
    }

    // ── LoginDtoValidator ─────────────────────────────────��──────────────────

    [Fact]
    public async Task LoginDtoValidator_WithValidCredentials_PassesValidation()
    {
        var validator = new LoginDtoValidator();
        var result = await validator.ValidateAsync(new LoginDto { Email = "user@example.com", Password = "anypass" });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task LoginDtoValidator_WithEmptyEmail_FailsWithEmailError()
    {
        var validator = new LoginDtoValidator();
        var result = await validator.ValidateAsync(new LoginDto { Email = "", Password = "anypass" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task LoginDtoValidator_WithInvalidEmail_FailsWithEmailError()
    {
        var validator = new LoginDtoValidator();
        var result = await validator.ValidateAsync(new LoginDto { Email = "invalid", Password = "anypass" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task LoginDtoValidator_WithEmptyPassword_FailsWithPasswordError()
    {
        var validator = new LoginDtoValidator();
        var result = await validator.ValidateAsync(new LoginDto { Email = "user@example.com", Password = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // ── OAuth2LoginDtoValidator ──────────────────────────────���───────────────

    [Theory]
    [InlineData("Google")]
    [InlineData("Microsoft")]
    [InlineData("GitHub")]
    public async Task OAuth2LoginDtoValidator_WithAllowedProvider_PassesValidation(string provider)
    {
        var validator = new OAuth2LoginDtoValidator();
        var result = await validator.ValidateAsync(new OAuth2LoginDto { Provider = provider, AccessToken = "token123" });
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("google")]
    [InlineData("GITHUB")]
    [InlineData("microsoft")]
    public async Task OAuth2LoginDtoValidator_WithProviderCaseInsensitive_PassesValidation(string provider)
    {
        var validator = new OAuth2LoginDtoValidator();
        var result = await validator.ValidateAsync(new OAuth2LoginDto { Provider = provider, AccessToken = "token123" });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task OAuth2LoginDtoValidator_WithUnknownProvider_FailsWithProviderError()
    {
        var validator = new OAuth2LoginDtoValidator();
        var result = await validator.ValidateAsync(new OAuth2LoginDto { Provider = "Facebook", AccessToken = "token123" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Provider");
    }

    [Fact]
    public async Task OAuth2LoginDtoValidator_WithEmptyProvider_FailsWithProviderError()
    {
        var validator = new OAuth2LoginDtoValidator();
        var result = await validator.ValidateAsync(new OAuth2LoginDto { Provider = "", AccessToken = "token123" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Provider");
    }

    [Fact]
    public async Task OAuth2LoginDtoValidator_WithEmptyAccessToken_FailsWithAccessTokenError()
    {
        var validator = new OAuth2LoginDtoValidator();
        var result = await validator.ValidateAsync(new OAuth2LoginDto { Provider = "Google", AccessToken = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AccessToken");
    }

    // ── ChangePasswordDtoValidator ─────────────────────────────────��─────────

    [Fact]
    public async Task ChangePasswordDtoValidator_WithValidDto_PassesValidation()
    {
        var validator = new ChangePasswordDtoValidator();
        var result = await validator.ValidateAsync(new ChangePasswordDto
        {
            CurrentPassword = "oldpass",
            NewPassword = "NewPass1!"
        });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordDtoValidator_WithEmptyCurrentPassword_FailsWithCurrentPasswordError()
    {
        var validator = new ChangePasswordDtoValidator();
        var result = await validator.ValidateAsync(new ChangePasswordDto
        {
            CurrentPassword = "",
            NewPassword = "NewPass1!"
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CurrentPassword");
    }

    [Fact]
    public async Task ChangePasswordDtoValidator_WithWeakNewPassword_FailsPasswordRules()
    {
        var validator = new ChangePasswordDtoValidator();
        var result = await validator.ValidateAsync(new ChangePasswordDto
        {
            CurrentPassword = "oldpass",
            NewPassword = "weak"
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task ChangePasswordDtoValidator_WithStrongNewPassword_PassesAllRules()
    {
        var validator = new ChangePasswordDtoValidator();
        var result = await validator.ValidateAsync(new ChangePasswordDto
        {
            CurrentPassword = "oldpass",
            NewPassword = "NewSecure1!"
        });
        result.Errors.Where(e => e.PropertyName == "NewPassword").Should().BeEmpty();
    }
}
