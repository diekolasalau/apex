namespace StudyMgt.Data.Entities;

public class TutorPortalAccountEntity
{
    public int Id { get; set; }
    public int TutorOnboardingId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; set; }

    public TutorOnboardingEntity TutorOnboarding { get; set; } = default!;
}