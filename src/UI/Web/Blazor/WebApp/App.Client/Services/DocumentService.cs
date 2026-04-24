using BlazorApp.Client.Services.Base;
using ProjectTemplate.Shared.Models;

namespace BlazorApp.Client.Services;

public interface IDocumentService
{
    Task<PagedResponse<DocumentResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default);
}

public class DocumentService(IHttpClientFactory httpClientFactory, LocalStorageService localStorage) 
    : BaseService(httpClientFactory, localStorage, "api/v1/document"), IDocumentService
{
    public Task<PagedResponse<DocumentResponseDto>> GetPagedAsync(int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return GetAsync<PagedResponse<DocumentResponseDto>>($"{ResourcePath}?pageNumber={page}&pageSize={pageSize}", ct);
    }
}
