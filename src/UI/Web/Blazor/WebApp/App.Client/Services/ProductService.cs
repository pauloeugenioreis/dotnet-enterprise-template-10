using BlazorApp.Client.Services.Base;
using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services;

public interface IProductService
{
    Task<PagedResponse<ProductResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task CreateAsync(CreateProductRequest dto, CancellationToken ct = default);
    Task UpdateAsync(long id, UpdateProductRequest dto, CancellationToken ct = default);
}

public class ProductService(HttpClient http) 
    : BaseService(http, "api/v1/product"), IProductService
{
    public Task<PagedResponse<ProductResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null, CancellationToken ct = default)
    {
        return GetPagedAsync<ProductResponseDto>(new { page, pageSize, searchTerm, isActive }, ct);
    }

    public Task CreateAsync(CreateProductRequest dto, CancellationToken ct = default)
    {
        return PostAsync<CreateProductRequest, ProductResponseDto>(ResourcePath!, dto, ct);
    }

    public Task UpdateAsync(long id, UpdateProductRequest dto, CancellationToken ct = default)
    {
        return PutAsync($"{ResourcePath}/{id}", dto, ct);
    }
}
