using System.ComponentModel.DataAnnotations;

namespace ProjectTemplate.Domain.Validators;

/// <summary>
/// Custom validation attribute for Redis connection strings
/// </summary>
public class RedisConnectionStringAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string connectionString)
        {
            return ValidationResult.Success;
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new ValidationResult("Redis connection string cannot be empty");
        }

        // Validar formato básico: host:port ou host:port,ssl=true
        if (!connectionString.Contains(':'))
        {
            return new ValidationResult(
                "Redis connection string must contain host and port (e.g., 'localhost:6379')");
        }

        return ValidationResult.Success;
    }
}
