using EnterpriseTemplate.MauiApp.Services.Base;
using ProjectTemplate.Shared.Models;

namespace EnterpriseTemplate.MauiApp.Services;

public interface IAuditService
{
    Task<PagedResponse<AuditLogDto>?> GetAuditLogsAsync(string entityType = null, string eventType = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
}

public class AuditService : BaseService, IAuditService
{
    public AuditService(HttpClient http) : base(http) { }

    public Task<PagedResponse<AuditLogDto>?> GetAuditLogsAsync(string entityType = null, string eventType = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var url = $"api/v1/audit?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(entityType)) url += $"&entityType={entityType}";
        if (!string.IsNullOrEmpty(eventType)) url += $"&eventType={eventType}";
        
        return GetAsync<PagedResponse<AuditLogDto>>(url, cancellationToken);
    }
}
