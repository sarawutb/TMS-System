using System.Text.Json;
using Microsoft.JSInterop;

namespace TmsSystem.BlazorWasm.Services;

public sealed class LocalStorageService(IJSRuntime jsRuntime)
{
    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        if (string.IsNullOrWhiteSpace(json)) return default;
        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task RemoveItemAsync(string key)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
