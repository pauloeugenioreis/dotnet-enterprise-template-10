using ProjectTemplate.Shared.Models;
using ProjectTemplate.Domain.Entities;
using BlazorApp.Client.Services.Base;

namespace BlazorApp.Client.Services;

public interface IAuditService
{
    Task<PagedResponse<DomainEvent>> GetAuditLogsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? entityType = null,
        string? eventType = null,
        string? userId = null,
        DateTime? from = null,
        DateTime? toDate = null);
    Task<object?> GetStatisticsAsync();
}

public class AuditService(IHttpClientFactory httpFactory) 
    : BaseService(httpFactory, "api/v1/audit"), IAuditService
{
    public Task<PagedResponse<DomainEvent>> GetAuditLogsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? entityType = null,
        string? eventType = null,
        string? userId = null,
        DateTime? from = null,
        DateTime? toDate = null)
    {
        var basePath = !string.IsNullOrEmpty(entityType) ? $"{ResourcePath}/{entityType}" : ResourcePath!;
        return GetPagedAsync<DomainEvent>(new { page, pageSize, eventType, userId, from, toDate }, default);
    }

    public Task<object?> GetStatisticsAsync()
        => GetAsync<object?>($"{ResourcePath}/statistics");
}
