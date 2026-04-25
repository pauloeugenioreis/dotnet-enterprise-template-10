using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Entities;
using BlazorApp.Client.Services.Base;

namespace BlazorApp.Client.Services;

public interface IAuditService
{
    Task<PagedResponse<DomainEvent>?> GetAuditLogsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? entityType = null,
        string? eventType = null,
        string? userId = null,
        DateTime? from = null,
        DateTime? toDate = null);
    Task<object?> GetStatisticsAsync();
}

public class AuditService(IHttpClientFactory httpFactory, LocalStorageService localStorage) 
    : Base.BaseService(httpFactory, localStorage, "api/v1/audit"), IAuditService
{
    public Task<PagedResponse<DomainEvent>?> GetAuditLogsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? entityType = null,
        string? eventType = null,
        string? userId = null,
        DateTime? from = null,
        DateTime? toDate = null)
    {
        var url = $"{ResourcePath}?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(entityType)) url = $"{ResourcePath}/{entityType}?page={page}&pageSize={pageSize}";
        
        if (!string.IsNullOrEmpty(eventType)) url += $"&eventType={eventType}";
        if (!string.IsNullOrEmpty(userId)) url += $"&userId={userId}";
        if (from.HasValue) url += $"&from={from.Value:yyyy-MM-dd}";
        if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";
        
        return GetAsync<PagedResponse<DomainEvent>>(url);
    }

    public Task<object?> GetStatisticsAsync()
        => GetAsync<object?>($"{ResourcePath}/statistics");
}
