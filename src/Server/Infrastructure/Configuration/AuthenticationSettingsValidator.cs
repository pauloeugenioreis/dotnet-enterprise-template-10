using Microsoft.Extensions.Options;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Infrastructure.Configuration;

/// <summary>
/// Validates authentication settings at startup
/// </summary>
public class AuthenticationSettingsValidator : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        var authSettings = options.Authentication;

        if (!authSettings.Enabled)
        {
            // Authentication disabled is valid
            return ValidateOptionsResult.Success;
        }

        // Validate JWT Secret
        if (string.IsNullOrWhiteSpace(authSettings.Jwt.Secret))
        {
            return ValidateOptionsResult.Fail(
                "JWT Secret is not configured. Please set 'AppSettings:Authentication:Jwt:Secret' in appsettings.json with a secure key of at least 32 characters.");
        }

        if (authSettings.Jwt.Secret.Length < 32)
        {
            return ValidateOptionsResult.Fail(
                $"JWT Secret must be at least 32 characters long for security. Current length: {authSettings.Jwt.Secret.Length}");
        }

        // Validate Issuer and Audience if validation is enabled
        if (authSettings.Jwt.ValidateIssuer && string.IsNullOrWhiteSpace(authSettings.Jwt.Issuer))
        {
            return ValidateOptionsResult.Fail("JWT Issuer is required when ValidateIssuer is true.");
        }

        if (authSettings.Jwt.ValidateAudience && string.IsNullOrWhiteSpace(authSettings.Jwt.Audience))
        {
            return ValidateOptionsResult.Fail("JWT Audience is required when ValidateAudience is true.");
        }

        return ValidateOptionsResult.Success;
    }
}
