namespace StudyMgt.Data.Entities;

public class CentreAdminPortalAccountEntity
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntilUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime? LastPasswordChangedUtc { get; set; }

    // Navigation property
    public ICollection<CentreAdminPasswordResetTokenEntity> PasswordResetTokens { get; set; } = new List<CentreAdminPasswordResetTokenEntity>();
}
