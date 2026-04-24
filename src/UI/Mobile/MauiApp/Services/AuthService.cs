using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.Shared.Models;

namespace EnterpriseTemplate.MauiApp.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
}

public class AuthService : BaseService, IAuthService
{
    public AuthService(HttpClient http) : base(http) { }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<LoginRequestDto, AuthResponseDto>("api/auth/login", request, cancellationToken);

        if (result?.Token is not null)
        {
            await AuthTokenHandler.SaveTokenAsync(result.Token);
        }

        return result;
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            await PostAsync<RegisterRequestDto, object>("api/auth/register", request, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
