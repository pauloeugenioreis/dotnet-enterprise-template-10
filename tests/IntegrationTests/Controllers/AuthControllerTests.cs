using ProjectTemplate.Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Exceptions;
using ProjectTemplate.Domain.Interfaces;
using ProjectTemplate.IntegrationTests.Support;
using Xunit;

namespace ProjectTemplate.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for AuthController using a deterministic in-memory auth service.
/// </summary>
[Collection("Integration Tests")]
public class AuthControllerTests
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactoryFixture factory)
    {
        factory.ClearEventStore();
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAuthService>();
                services.AddSingleton<IAuthService, InMemoryAuthService>();
            });
        });

        _client = customFactory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName);
    }

    [Fact]
    public async Task Register_ValidPayload_ReturnsOk()
    {
        // Arrange
        var request = new RegisterDto
        {
            Email = "integration_user@example.com",
            Password = "Test@123",
            FirstName = "Integration",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBeNullOrWhiteSpace();
        payload.User.Email.Should().Be("integration_user@example.com");
    }

    [Fact]
    public async Task Login_ValidPayload_ReturnsOk()
    {
        // Arrange
        var request = new LoginDto
        {
            Email = "integration_user@example.com",
            Password = "Test@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        payload.Should().NotBeNull();
        payload!.User.Email.Should().Be("integration_user@example.com");
    }

    [Fact]
    public async Task OAuth2Login_ValidPayload_ReturnsOk()
    {
        // Arrange
        var request = new OAuth2LoginDto
        {
            Provider = "Google",
            AccessToken = "fake-provider-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/oauth2/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        payload.Should().NotBeNull();
        payload!.User.Email.Should().Be("oauth_google@example.com");
    }

    [Fact]
    public async Task RefreshToken_ValidPayload_ReturnsOk()
    {
        // Arrange
        var request = new RefreshTokenDto
        {
            RefreshToken = "refresh-token-123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/refresh-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        payload.Should().NotBeNull();
        payload!.RefreshToken.Should().Be("refresh-token-123");
    }

    [Fact]
    public async Task RevokeToken_ValidPayload_ReturnsNoContent()
    {
        // Arrange
        var request = new RefreshTokenDto
        {
            RefreshToken = "refresh-token-123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/revoke-token", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetCurrentUser_Authorized_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<UserDto>();
        payload.Should().NotBeNull();
        payload!.Id.Should().Be(123);
        payload.Email.Should().Be("integration_user@example.com");
    }

    [Fact]
    public async Task ChangePassword_Authorized_ReturnsNoContent()
    {
        // Arrange
        var request = new ChangePasswordDto
        {
            CurrentPassword = "Test@123",
            NewPassword = "NewTest@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateProfile_Authorized_ReturnsOk()
    {
        // Arrange
        var request = new UpdateProfileDto
        {
            FirstName = "Updated",
            LastName = "Name",
            PhoneNumber = "+5511999999999",
            ProfileImageUrl = "https://example.com/profile.png"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/Auth/profile", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<UserDto>();
        payload.Should().NotBeNull();
        payload!.FirstName.Should().Be("Updated");
        payload.LastName.Should().Be("Name");
    }

    private sealed class InMemoryAuthService : IAuthService
    {
        private readonly UserDto _defaultUser = new()
        {
            Id = 123,
            Email = "integration_user@example.com",
            FirstName = "Integration",
            LastName = "User",
            FullName = "Integration User",
            Roles = ["User"],
            CreatedAt = DateTime.UtcNow
        };

        public Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
        {
            var user = CloneUser(_defaultUser);
            user.Email = dto.Email;
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.FullName = $"{dto.FirstName} {dto.LastName}".Trim();

            return Task.FromResult(CreateAuthResponse(user));
        }

        public Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            var user = CloneUser(_defaultUser);
            user.Email = dto.Email;
            return Task.FromResult(CreateAuthResponse(user));
        }

        public Task<AuthResponseDto> OAuth2LoginAsync(OAuth2LoginDto dto, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            var user = CloneUser(_defaultUser);
            user.Email = $"oauth_{dto.Provider.ToLowerInvariant()}@example.com";

            return Task.FromResult(CreateAuthResponse(user));
        }

        public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            var response = CreateAuthResponse(_defaultUser);
            response.RefreshToken = refreshToken;
            return Task.FromResult(response);
        }

        public Task RevokeTokenAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new NotFoundException("Refresh token not found");
            }

            return Task.CompletedTask;
        }

        public Task<UserDto> GetCurrentUserAsync(long userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_defaultUser);
        }

        public Task ChangePasswordAsync(long userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<UserDto> UpdateProfileAsync(long userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
        {
            var user = CloneUser(_defaultUser);
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.ProfileImageUrl = dto.ProfileImageUrl;
            user.FullName = $"{dto.FirstName} {dto.LastName}".Trim();

            return Task.FromResult(user);
        }

        private static UserDto CloneUser(UserDto source)
        {
            return new UserDto
            {
                Id = source.Id,
                Email = source.Email,
                FirstName = source.FirstName,
                LastName = source.LastName,
                FullName = source.FullName,
                ProfileImageUrl = source.ProfileImageUrl,
                Roles = [.. source.Roles],
                CreatedAt = source.CreatedAt
            };
        }

        private static AuthResponseDto CreateAuthResponse(UserDto user)
        {
            return new AuthResponseDto
            {
                AccessToken = "access-token-123",
                RefreshToken = "refresh-token-123",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = user
            };
        }
    }
}
