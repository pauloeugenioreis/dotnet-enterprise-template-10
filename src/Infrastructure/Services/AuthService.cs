using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectTemplate.Domain;
using ProjectTemplate.Domain.Dtos;
using ProjectTemplate.Domain.Entities;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.Infrastructure.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly PasswordPolicySettings _passwordPolicy;
    private readonly AuthenticationSettings _authSettings;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IOptions<AuthenticationSettings> authSettings)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _authSettings = authSettings.Value;
        _passwordPolicy = authSettings.Value.PasswordPolicy;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        // Validate password policy
        ValidatePassword(dto.Password);

        // Check if user already exists
        if (await _userRepository.ExistsAsync(dto.Username, dto.Email, cancellationToken))
        {
            throw new DomainException("Username or email already exists");
        }

        // Create user
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        user = await _userRepository.CreateAsync(user, cancellationToken);

        // Assign default role
        await _userRepository.AddToRoleAsync(user.Id, "User", cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken("0.0.0.0");
        refreshToken.UserId = user.Id;

        await _userRepository.SaveRefreshTokenAsync(refreshToken, cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_authSettings.Jwt.ExpirationMinutes),
            User = await MapToUserDto(user, cancellationToken)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(dto.Username, cancellationToken);

        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is disabled");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(ipAddress ?? "0.0.0.0");
        refreshToken.UserId = user.Id;

        // Remove old refresh tokens
        await _userRepository.RemoveOldRefreshTokensAsync(user.Id, cancellationToken);
        await _userRepository.SaveRefreshTokenAsync(refreshToken, cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_authSettings.Jwt.ExpirationMinutes),
            User = await MapToUserDto(user, cancellationToken)
        };
    }

    public async Task<AuthResponseDto> OAuth2LoginAsync(OAuth2LoginDto dto, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        // TODO: Implement OAuth2 verification with external provider
        // This is a simplified version - in production, you should verify the access token with the provider

        throw new NotImplementedException("OAuth2 login requires external provider verification");

        // Example flow:
        // 1. Verify access token with provider (Google, Microsoft, GitHub)
        // 2. Get user info from provider
        // 3. Check if user exists by external ID
        // 4. If not, create new user
        // 5. Generate tokens and return
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var user = await _tokenService.GetUserByRefreshTokenAsync(refreshToken, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var oldToken = await _userRepository.GetRefreshTokenAsync(refreshToken, cancellationToken);

        if (oldToken == null || !oldToken.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress ?? "0.0.0.0");
        newRefreshToken.UserId = user.Id;

        // Revoke old token and save new one
        await _userRepository.RevokeRefreshTokenAsync(
            refreshToken,
            ipAddress,
            newRefreshToken.Token,
            cancellationToken);

        await _userRepository.SaveRefreshTokenAsync(newRefreshToken, cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_authSettings.Jwt.ExpirationMinutes),
            User = await MapToUserDto(user, cancellationToken)
        };
    }

    public async Task RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var token = await _userRepository.GetRefreshTokenAsync(refreshToken, cancellationToken);

        if (token == null)
        {
            throw new NotFoundException("Refresh token not found");
        }

        if (!token.IsActive)
        {
            throw new DomainException("Token is already revoked or expired");
        }

        await _userRepository.RevokeRefreshTokenAsync(refreshToken, ipAddress, null, cancellationToken);
    }

    public async Task<UserDto> GetCurrentUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        return await MapToUserDto(user, cancellationToken);
    }

    public async Task ChangePasswordAsync(long userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        // Verify current password
        if (!VerifyPassword(dto.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        // Validate new password
        ValidatePassword(dto.NewPassword);

        // Update password
        user.PasswordHash = HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<UserDto> UpdateProfileAsync(long userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.FirstName = dto.FirstName ?? user.FirstName;
        user.LastName = dto.LastName ?? user.LastName;
        user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
        user.ProfileImageUrl = dto.ProfileImageUrl ?? user.ProfileImageUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return await MapToUserDto(user, cancellationToken);
    }

    #region Private Methods

    private void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new DomainException("Password is required");
        }

        if (password.Length < _passwordPolicy.MinimumLength)
        {
            throw new DomainException($"Password must be at least {_passwordPolicy.MinimumLength} characters long");
        }

        if (_passwordPolicy.RequireDigit && !password.Any(char.IsDigit))
        {
            throw new DomainException("Password must contain at least one digit");
        }

        if (_passwordPolicy.RequireLowercase && !password.Any(char.IsLower))
        {
            throw new DomainException("Password must contain at least one lowercase letter");
        }

        if (_passwordPolicy.RequireUppercase && !password.Any(char.IsUpper))
        {
            throw new DomainException("Password must contain at least one uppercase letter");
        }

        if (_passwordPolicy.RequireNonAlphanumeric && !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            throw new DomainException("Password must contain at least one special character");
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        var hash = HashPassword(password);
        return hash == passwordHash;
    }

    private async Task<UserDto> MapToUserDto(User user, CancellationToken cancellationToken)
    {
        var roles = await _userRepository.GetUserRolesAsync(user.Id, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            ProfileImageUrl = user.ProfileImageUrl,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };
    }

    #endregion
}
