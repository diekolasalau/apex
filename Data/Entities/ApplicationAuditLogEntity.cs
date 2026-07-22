namespace StudyMgt.Data.Entities;

public class ApplicationAuditLogEntity
{
    public long Id { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = "Action";
    public string Action { get; set; } = string.Empty;
    public string? PagePath { get; set; }
    public string? ActorRole { get; set; }
    public string? ActorUsername { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public bool Success { get; set; } = true;
    public string? Details { get; set; }
}
