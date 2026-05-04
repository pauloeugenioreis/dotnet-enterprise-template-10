using Microsoft.AspNetCore.Identity;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Infrastructure.Services;

/// <summary>
/// Password hasher backed by ASP.NET Core Identity's <see cref="PasswordHasher{TUser}"/>.
/// Uses PBKDF2 with HMAC-SHA512, 100,000 iterations and a 128-bit salt by default
/// (Identity v3 format).
/// </summary>
public sealed class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _hasher = new();
    private static readonly User HashContext = new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _hasher.HashPassword(HashContext, password);
    }

    public bool Verify(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
        {
            return false;
        }

        var result = _hasher.VerifyHashedPassword(HashContext, hashedPassword, password);
        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
