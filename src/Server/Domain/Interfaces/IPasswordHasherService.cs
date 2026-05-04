namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Abstraction for secure password hashing.
/// Implementations must use a slow, salted algorithm (PBKDF2, BCrypt, Argon2).
/// </summary>
public interface IPasswordHasherService
{
    /// <summary>
    /// Hashes a plaintext password producing a self-contained string
    /// that includes algorithm parameters and salt.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a plaintext password against a previously stored hash.
    /// </summary>
    /// <returns>
    /// <c>true</c> when the password matches the stored hash; <c>false</c> otherwise.
    /// </returns>
    bool Verify(string password, string hashedPassword);
}
