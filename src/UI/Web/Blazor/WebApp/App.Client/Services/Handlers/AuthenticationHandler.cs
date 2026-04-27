using System.Net.Http.Headers;
using BlazorApp.Client.Services.Base;

namespace BlazorApp.Client.Services.Handlers;

public class AuthenticationHandler(LocalStorageService localStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (Exception)
        {
            // Pode falhar durante o pre-rendering pois o JS Interop não está disponível
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
