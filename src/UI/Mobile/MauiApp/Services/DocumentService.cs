using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.SharedModels;

namespace EnterpriseTemplate.MauiApp.Services;

public interface IDocumentService
{
    Task<PagedResponse<DocumentResponseDto>?> GetDocumentsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
}

public class DocumentService : BaseService, IDocumentService
{
    public DocumentService(HttpClient http) : base(http) { }

    public Task<PagedResponse<DocumentResponseDto>?> GetDocumentsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => GetAsync<PagedResponse<DocumentResponseDto>>($"api/documents?page={page}&pageSize={pageSize}", cancellationToken);
}
