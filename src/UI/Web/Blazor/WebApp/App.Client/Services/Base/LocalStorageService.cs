using Microsoft.JSInterop;

namespace BlazorApp.Client.Services.Base;

public class LocalStorageService(IJSRuntime js)
{
    public async ValueTask SetItemAsync<T>(string key, T value)
    {
        await js.InvokeVoidAsync("localStorage.setItem", key, System.Text.Json.JsonSerializer.Serialize(value));
    }

    public async ValueTask<T?> GetItemAsync<T>(string key)
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", key);
        return string.IsNullOrEmpty(json) ? default : System.Text.Json.JsonSerializer.Deserialize<T>(json);
    }

    public async ValueTask RemoveItemAsync(string key)
    {
        await js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
