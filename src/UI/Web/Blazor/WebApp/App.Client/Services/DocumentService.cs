using BlazorApp.Client.Services.Base;
using ProjectTemplate.SharedModels;

namespace BlazorApp.Client.Services;

public interface IDocumentService
{
    Task<PagedResponse<DocumentResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
}

public class DocumentService(IHttpClientFactory httpClientFactory) 
    : BaseService(httpClientFactory), IDocumentService
{
    public Task<PagedResponse<DocumentResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return GetAsync<PagedResponse<DocumentResponseDto>>($"api/v1/document?pageNumber={page}&pageSize={pageSize}", ct);
    }
}
