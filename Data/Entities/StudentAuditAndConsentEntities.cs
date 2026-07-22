namespace StudyMgt.Data.Entities;

public class StudentOnboardingAuditEntity
{
    public int Id { get; set; }
    public int StudentOnboardingId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? FieldChanged { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }

    public StudentOnboardingEntity StudentOnboarding { get; set; } = default!;
}

public class StudentConsentRecordEntity
{
    public int Id { get; set; }
    public int StudentOnboardingId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public bool IsConsented { get; set; }
    public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public StudentOnboardingEntity StudentOnboarding { get; set; } = default!;
}
