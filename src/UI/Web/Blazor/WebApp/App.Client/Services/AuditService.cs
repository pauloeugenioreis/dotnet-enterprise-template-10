using ProjectTemplate.SharedModels;
using ProjectTemplate.Domain.Entities;
using BlazorApp.Client.Services.Base;

namespace BlazorApp.Client.Services;

public interface IAuditService
{
    Task<PagedResponse<DomainEvent>?> GetAuditLogsAsync(int page = 1, int pageSize = 10, string? entityType = null);
    Task<object?> GetStatisticsAsync();
}

public class AuditService(IHttpClientFactory httpFactory, LocalStorageService localStorage) 
    : Base.BaseService(httpFactory, localStorage, "api/v1/audit"), IAuditService
{
    public Task<PagedResponse<DomainEvent>?> GetAuditLogsAsync(int page = 1, int pageSize = 10, string? entityType = null)
    {
        var url = $"{ResourcePath}?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(entityType)) url = $"{ResourcePath}/{entityType}?page={page}&pageSize={pageSize}";
        return GetAsync<PagedResponse<DomainEvent>>(url);
    }

    public Task<object?> GetStatisticsAsync()
        => GetAsync<object?>($"{ResourcePath}/statistics");
}
