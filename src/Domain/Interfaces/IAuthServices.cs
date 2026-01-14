using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Domain.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> OAuth2LoginAsync(OAuth2LoginDto dto, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(long userId, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(long userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateProfileAsync(long userId, UpdateProfileDto dto, CancellationToken cancellationToken = default);
}

/// <summary>
/// JWT Token service interface
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(string ipAddress);
    Task<long?> ValidateAccessTokenAsync(string token);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByExternalProviderAsync(string provider, string externalId, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string username, string email, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserRolesAsync(long userId, CancellationToken cancellationToken = default);
    Task AddToRoleAsync(long userId, string roleName, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task SaveRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, string? ipAddress, string? replacedByToken = null, CancellationToken cancellationToken = default);
    Task RemoveOldRefreshTokensAsync(long userId, CancellationToken cancellationToken = default);
}
