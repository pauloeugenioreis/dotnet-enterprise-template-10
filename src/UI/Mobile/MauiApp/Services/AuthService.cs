using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.Shared.Models;

namespace EnterpriseTemplate.MauiApp.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
    Task<bool> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
}

public class AuthService : BaseService, IAuthService
{
    public AuthService(HttpClient http) : base(http) { }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<LoginDto, AuthResponseDto>("api/auth/login", request, cancellationToken);

        if (result?.AccessToken is not null)
        {
            await AuthTokenHandler.SaveTokenAsync(result.AccessToken);
        }

        return result;
    }

    public async Task<bool> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            await PostAsync<RegisterDto, object>("api/auth/register", request, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
