using System.Net.Http.Json;

namespace BlazorApp.Client.Services.Base;

public abstract class BaseService(IHttpClientFactory httpClientFactory, string clientName = "api")
{
    protected readonly HttpClient Http = httpClientFactory.CreateClient(clientName);

    protected async Task<T> GetAsync<T>(string url, CancellationToken ct = default)
    {
        var response = await Http.GetFromJsonAsync<T>(url, ct);
        return response ?? throw new InvalidOperationException("Response was null");
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken ct = default)
    {
        var response = await Http.PostAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct)) 
               ?? throw new InvalidOperationException("Response was null");
    }
}
