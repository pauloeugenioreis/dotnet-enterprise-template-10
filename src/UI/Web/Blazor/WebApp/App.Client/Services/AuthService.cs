using BlazorApp.Client.Services.Base;
using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto request);
    Task<bool> RegisterAsync(RegisterDto request);
}

public class AuthService(IHttpClientFactory httpClientFactory, LocalStorageService localStorage) 
    : BaseService(httpClientFactory, localStorage, "api/v1/auth"), IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginDto request)
    {
        try 
        {
            return await PostAsync<LoginDto, AuthResponseDto>($"{ResourcePath}/login", request);
        }
        catch 
        {
            return null;
        }
    }

    public async Task<bool> RegisterAsync(RegisterDto request)
    {
        try
        {
            await PostAsync<RegisterDto, object>($"{ResourcePath}/register", request);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
