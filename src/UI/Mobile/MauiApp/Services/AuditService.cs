using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.SharedModels;

namespace EnterpriseTemplate.MauiApp.Services;

public interface IAuditService
{
    Task<PagedResponse<DomainEvent>?> GetAuditLogsAsync(string entityType, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
}

public class AuditService : BaseService, IAuditService
{
    public AuditService(HttpClient http) : base(http) { }

    public Task<PagedResponse<DomainEvent>?> GetAuditLogsAsync(string entityType, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => GetAsync<PagedResponse<DomainEvent>>($"api/v1/audit/{entityType}?page={page}&pageSize={pageSize}", cancellationToken);
}
