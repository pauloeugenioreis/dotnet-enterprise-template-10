using BlazorApp.Client.Services.Base;
using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services;

public interface IProductService
{
    Task<PagedResponse<ProductResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}

public class ProductService(IHttpClientFactory httpClientFactory, LocalStorageService localStorage) 
    : BaseService(httpClientFactory, localStorage, "api/v1/product"), IProductService
{
    public Task<PagedResponse<ProductResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null, CancellationToken ct = default)
    {
        var url = $"{ResourcePath}?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm)) url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        if (isActive.HasValue) url += $"&isActive={isActive.Value}";
        return GetAsync<PagedResponse<ProductResponseDto>>(url, ct);
    }
}
