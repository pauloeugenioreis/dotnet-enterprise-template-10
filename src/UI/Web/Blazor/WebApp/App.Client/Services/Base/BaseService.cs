using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BlazorApp.Client.Services.Base;

public abstract class BaseService(IHttpClientFactory httpClientFactory, LocalStorageService localStorage, string? resourcePath = null)
{
    protected readonly HttpClient Http = httpClientFactory.CreateClient("ApiGateway");
    protected readonly string? ResourcePath = resourcePath;

    private async Task AddAuthHeaderAsync()
    {
        try 
        {
            var token = await localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (InvalidOperationException)
        {
            // JS Interop não disponível (pre-rendering)
        }
    }

    protected async Task<T> GetAsync<T>(string url, CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await Http.GetFromJsonAsync<T>(url, ct);
        return response ?? throw new InvalidOperationException("Response was null");
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await Http.PostAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct)) 
               ?? throw new InvalidOperationException("Response was null");
    }

    protected async Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await Http.PutAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct)) 
               ?? throw new InvalidOperationException("Response was null");
    }

    protected async Task PutAsync<TRequest>(string url, TRequest request, CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await Http.PutAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();
    }

    public virtual Task DeleteAsync(long id, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(ResourcePath))
            throw new InvalidOperationException("ResourcePath must be set to use generic DeleteAsync.");

        return DeleteRequestAsync($"{ResourcePath}/{id}", ct);
    }

    protected async Task DeleteRequestAsync(string url, CancellationToken ct = default)
    {
        await AddAuthHeaderAsync();
        var response = await Http.DeleteAsync(url, ct);
        response.EnsureSuccessStatusCode();
    }
}
