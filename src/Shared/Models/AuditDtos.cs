namespace ProjectTemplate.Shared.Models;

public class AuditLogDto
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public string AggregateId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime OccurredOn { get; set; }
    public int Version { get; set; }
    public object? EventData { get; set; }
    public string? Metadata { get; set; }
}
