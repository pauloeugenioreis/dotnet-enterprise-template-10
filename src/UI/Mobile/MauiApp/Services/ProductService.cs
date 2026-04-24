using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.Shared.Models;

namespace EnterpriseTemplate.MauiApp.Services;

public interface IProductService
{
    Task<PagedResponse<ProductResponseDto>?> GetProductsAsync(int page = 1, int pageSize = 10, string searchTerm = null, bool? isActive = null, CancellationToken cancellationToken = default);
}

public class ProductService : BaseService, IProductService
{
    public ProductService(HttpClient http) : base(http) { }

    public Task<PagedResponse<ProductResponseDto>?> GetProductsAsync(int page = 1, int pageSize = 10, string searchTerm = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var url = $"api/products?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm)) url += $"&searchTerm={searchTerm}";
        if (isActive.HasValue) url += $"&isActive={isActive.Value}";
        
        return GetAsync<PagedResponse<ProductResponseDto>>(url, cancellationToken);
    }
}
