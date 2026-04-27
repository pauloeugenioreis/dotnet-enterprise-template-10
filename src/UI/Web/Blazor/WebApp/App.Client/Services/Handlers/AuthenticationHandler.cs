using System.Net.Http.Headers;
using BlazorApp.Client.Services.Base;
using BlazorApp.Client.Auth;
using Microsoft.JSInterop;

namespace BlazorApp.Client.Services.Handlers;

public class AuthenticationHandler(LocalStorageService localStorage, TokenProvider tokenProvider, IJSRuntime js) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(tokenProvider.Token))
            {
                if (js.GetType().Name != "UnsupportedJavaScriptRuntime")
                {
                    tokenProvider.Token = await localStorage.GetItemAsync<string>("authToken");
                }
            }

            Console.WriteLine($"[AuthHandler] Provider: {tokenProvider.GetHashCode()} | Token: {(string.IsNullOrEmpty(tokenProvider.Token) ? "NULL" : "SET")} | Req: {request.Method} {request.RequestUri}");

            if (!string.IsNullOrEmpty(tokenProvider.Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenProvider.Token);
            }
        }
        catch (Exception ex)
        {
            if (!ex.Message.Contains("JavaScript interop calls cannot be issued at this time"))
            {
                Console.WriteLine($"[AuthHandler] Erro ao obter token: {ex.Message}");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
