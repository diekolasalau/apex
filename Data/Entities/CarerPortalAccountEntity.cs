namespace StudyMgt.Data.Entities;

public class CarerPortalAccountEntity
{
    public int Id { get; set; }
    public int CarerOnboardingId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; set; }

    public CarerOnboardingEntity CarerOnboarding { get; set; } = default!;
}
