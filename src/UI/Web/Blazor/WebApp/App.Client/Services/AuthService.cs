using BlazorApp.Client.Services.Base;
using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto request);
    Task<bool> RegisterAsync(RegisterDto request);
}

public class AuthService(IHttpClientFactory httpClientFactory, LocalStorageService localStorage) 
    : BaseService(httpClientFactory, "api/v1/auth"), IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginDto request)
    {
        var response = await PostAsync<LoginDto, AuthResponseDto>($"{ResourcePath}/login", request);
        if (response != null)
        {
            await localStorage.SetItemAsync("authToken", response.AccessToken);
            await localStorage.SetItemAsync("authUser", response.User);
        }
        return response;
    }

    public async Task<bool> RegisterAsync(RegisterDto request)
    {
        await PostAsync<RegisterDto, object>($"{ResourcePath}/register", request);
        return true;
    }
}
