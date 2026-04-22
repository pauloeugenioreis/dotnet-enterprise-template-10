using BlazorApp.Client.Services.Base;
using ProjectTemplate.SharedModels;

namespace BlazorApp.Client.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto request);
    Task<bool> RegisterAsync(RegisterDto request);
}

public class AuthService(IHttpClientFactory httpClientFactory) 
    : BaseService(httpClientFactory), IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginDto request)
    {
        try 
        {
            return await PostAsync<LoginDto, AuthResponseDto>("api/v1/auth/login", request);
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
            await PostAsync<RegisterDto, object>("api/v1/auth/register", request);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
