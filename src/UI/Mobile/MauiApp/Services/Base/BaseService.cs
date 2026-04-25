using System.Net.Http.Json;

namespace EnterpriseTemplate.MauiApp.Services.Base;

/// <summary>
/// Serviço base que encapsula todas as chamadas HTTP.
/// Segue o mesmo padrão do BlazorWebApp/App.Client/Services/Base/BaseService.cs.
/// </summary>
public abstract class BaseService
{
    protected readonly HttpClient Http;

    protected BaseService(HttpClient http)
    {
        Http = http;
    }

    protected async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken cancellationToken = default)
    {
        var response = await Http.PostAsJsonAsync(url, data, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data, CancellationToken cancellationToken = default)
    {
        var response = await Http.PutAsJsonAsync(url, data, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    protected async Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        var response = await Http.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
