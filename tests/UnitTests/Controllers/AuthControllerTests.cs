using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectTemplate.Api.Controllers;
using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Interfaces;

namespace ProjectTemplate.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_mockAuthService.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidDto_ReturnsOkWithAuthResponse()
    {
        var dto = new RegisterDto { Email = "user@example.com", Password = "Password1!" };
        var authResponse = CreateAuthResponse();
        _mockAuthService.Setup(s => s.RegisterAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(authResponse);

        var result = await _controller.Register(dto, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(authResponse);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithAuthResponse()
    {
        var dto = new LoginDto { Email = "user@example.com", Password = "Password1!" };
        var authResponse = CreateAuthResponse();
        _mockAuthService
            .Setup(s => s.LoginAsync(dto, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var result = await _controller.Login(dto, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(authResponse);
    }

    [Fact]
    public async Task Login_ExtractsIpAddressFromXForwardedForHeader()
    {
        var dto = new LoginDto { Email = "user@example.com", Password = "pass" };
        _controller.ControllerContext.HttpContext.Request.Headers["X-Forwarded-For"] = "192.168.1.100";
        _mockAuthService
            .Setup(s => s.LoginAsync(dto, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateAuthResponse());

        await _controller.Login(dto, CancellationToken.None);

        _mockAuthService.Verify(s => s.LoginAsync(dto, "192.168.1.100", It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── OAuth2Login ───────────────────────────────────────────────────────────

    [Fact]
    public async Task OAuth2Login_WithValidDto_ReturnsOkWithAuthResponse()
    {
        var dto = new OAuth2LoginDto { Provider = "Google", AccessToken = "token123" };
        var authResponse = CreateAuthResponse();
        _mockAuthService
            .Setup(s => s.OAuth2LoginAsync(dto, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var result = await _controller.OAuth2Login(dto, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(authResponse);
    }

    // ── RefreshToken ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOkWithAuthResponse()
    {
        var dto = new RefreshTokenDto { RefreshToken = "refresh-token-value" };
        var authResponse = CreateAuthResponse();
        _mockAuthService
            .Setup(s => s.RefreshTokenAsync("refresh-token-value", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var result = await _controller.RefreshToken(dto, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(authResponse);
    }

    // ── RevokeToken ───────────────────────────────────────────────────────────

    [Fact]
    public async Task RevokeToken_WithValidToken_ReturnsNoContent()
    {
        var dto = new RefreshTokenDto { RefreshToken = "refresh-token-value" };
        _mockAuthService
            .Setup(s => s.RevokeTokenAsync("refresh-token-value", It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.RevokeToken(dto, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    // ── GetCurrentUser ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentUser_WithValidClaim_ReturnsOkWithUserDto()
    {
        SetAuthenticatedUser(42);
        var userDto = new UserDto { Id = 42, Email = "user@example.com" };
        _mockAuthService.Setup(s => s.GetCurrentUserAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(userDto);

        var result = await _controller.GetCurrentUser(CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(userDto);
    }

    [Fact]
    public async Task GetCurrentUser_WithMissingUserIdClaim_ThrowsUnauthorizedAccessException()
    {
        // No claim set — User is empty ClaimsPrincipal
        var act = () => _controller.GetCurrentUser(CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetCurrentUser_WithNonNumericUserIdClaim_ThrowsUnauthorizedAccessException()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "not-a-number")]));

        var act = () => _controller.GetCurrentUser(CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── ChangePassword ────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePassword_WithValidClaim_ReturnsNoContent()
    {
        SetAuthenticatedUser(42);
        var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "NewPass1!" };
        _mockAuthService.Setup(s => s.ChangePasswordAsync(42, dto, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.ChangePassword(dto, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_WithValidClaim_ReturnsOkWithUpdatedUser()
    {
        SetAuthenticatedUser(42);
        var dto = new UpdateProfileDto { FirstName = "John", LastName = "Doe" };
        var userDto = new UserDto { Id = 42, Email = "user@example.com", FullName = "John Doe" };
        _mockAuthService.Setup(s => s.UpdateProfileAsync(42, dto, It.IsAny<CancellationToken>())).ReturnsAsync(userDto);

        var result = await _controller.UpdateProfile(dto, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(userDto);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetAuthenticatedUser(long userId)
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())]));
    }

    private static AuthResponseDto CreateAuthResponse() => new()
    {
        AccessToken = "access-token",
        RefreshToken = "refresh-token",
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        User = new UserDto { Id = 1, Email = "user@example.com", FullName = "Test User" }
    };
}
