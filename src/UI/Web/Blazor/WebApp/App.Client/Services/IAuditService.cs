using ProjectTemplate.SharedModels;

namespace BlazorApp.Client.Services;

public interface IAuditService
{
    Task<PagedResponse<DomainEvent>?> GetAuditLogsAsync(string entityType, int page = 1, int pageSize = 10);
    Task<object?> GetStatisticsAsync();
}

public class AuditService : Base.BaseService, IAuditService
{
    public AuditService(HttpClient http) : base(http) { }

    public Task<PagedResponse<DomainEvent>?> GetAuditLogsAsync(string entityType, int page = 1, int pageSize = 10)
        => GetAsync<PagedResponse<DomainEvent>>($"api/v1/audit/{entityType}?page={page}&pageSize={pageSize}");

    public Task<object?> GetStatisticsAsync()
        => GetAsync<object>("api/v1/audit/statistics");
}
