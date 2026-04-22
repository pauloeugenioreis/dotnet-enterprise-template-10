using BlazorApp.Client.Services.Base;
using ProjectTemplate.SharedModels;

namespace BlazorApp.Client.Services;

public interface IProductService
{
    Task<PagedResponse<ProductResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
}

public class ProductService(IHttpClientFactory httpClientFactory) 
    : BaseService(httpClientFactory), IProductService
{
    public Task<PagedResponse<ProductResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return GetAsync<PagedResponse<ProductResponseDto>>($"api/v1/product?pageNumber={page}&pageSize={pageSize}", ct);
    }
}
