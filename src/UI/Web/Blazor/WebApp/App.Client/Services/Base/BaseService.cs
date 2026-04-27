using System.Net.Http.Json;
using BlazorApp.Client.Services.Utils;
using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services.Base;

public abstract class BaseService(HttpClient http, string? resourcePath = null)
{
    protected readonly HttpClient Http = http;
    protected readonly string? ResourcePath = resourcePath;

    protected async Task<T> GetAsync<T>(string url, CancellationToken ct = default)
    {
        var response = await Http.GetFromJsonAsync<T>(url, ct);
        return response ?? throw new InvalidOperationException("Response was null");
    }

    protected async Task<PagedResponse<T>> GetPagedAsync<T>(object? filters = null, CancellationToken ct = default)
    {
        var url = QueryStringHelper.ToQueryString(ResourcePath!, filters);
        return await GetAsync<PagedResponse<T>>(url, ct);
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken ct = default)
    {
        var response = await Http.PostAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct)) 
               ?? throw new InvalidOperationException("Response was null");
    }

    protected async Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken ct = default)
    {
        var response = await Http.PutAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct)) 
               ?? throw new InvalidOperationException("Response was null");
    }

    protected async Task PutAsync<TRequest>(string url, TRequest request, CancellationToken ct = default)
    {
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
        var response = await Http.DeleteAsync(url, ct);
        response.EnsureSuccessStatusCode();
    }

    protected async Task PatchAsync<TRequest>(string url, TRequest request, CancellationToken ct = default)
    {
        var response = await Http.PatchAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();
    }
}
