using System.Net.Http.Headers;
using Microsoft.Maui.Storage;

namespace EnterpriseTemplate.MauiApp.Services.Base;

/// <summary>
/// Handler HTTP que injeta o token JWT automaticamente em todas as requisições.
/// Equivalente ao LoadingHandler do BlazorWebApp, mas para MAUI.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private const string TokenKey = "auth_token";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    public static async Task SaveTokenAsync(string token)
        => await SecureStorage.Default.SetAsync(TokenKey, token);

    public static async Task<string?> GetTokenAsync()
        => await SecureStorage.Default.GetAsync(TokenKey);

    public static void ClearToken()
        => SecureStorage.Default.Remove(TokenKey);
}
