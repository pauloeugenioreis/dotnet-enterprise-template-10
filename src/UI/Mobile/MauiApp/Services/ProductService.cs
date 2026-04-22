using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.SharedModels;

namespace EnterpriseTemplate.MauiApp.Services;

public interface IProductService
{
    Task<PagedResponse<ProductResponseDto>?> GetProductsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
}

public class ProductService : BaseService, IProductService
{
    public ProductService(HttpClient http) : base(http) { }

    public Task<PagedResponse<ProductResponseDto>?> GetProductsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => GetAsync<PagedResponse<ProductResponseDto>>($"api/products?page={page}&pageSize={pageSize}", cancellationToken);
}
