namespace StudyMgt.Data.Entities;

public class CentreAdminPasswordResetTokenEntity
{
    public int Id { get; set; }
    public int CentreAdminPortalAccountId { get; set; }
    public string ResetToken { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAtUtc { get; set; }

    // Navigation property
    public CentreAdminPortalAccountEntity CentreAdminAccount { get; set; } = default!;
}
