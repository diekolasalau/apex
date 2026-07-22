namespace StudyMgt.Data.Entities;

public class CarerOnboardingEntity
{
    public int Id { get; set; }
    public string CarerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByUserId { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    public string? StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? Relationship { get; set; }
    public DateTime StudentDateOfBirth { get; set; }
    public string? EHCPStatus { get; set; }

    public bool HasParentalResponsibility { get; set; }
    public bool NoRestrictiveOrders { get; set; }

    public string? PreferredContactMethod { get; set; }
    public string? MedicalAndAccessibilityInfo { get; set; }
     public string? MedicalAndAccessibilityInfo2 { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }

    public bool PrivacyNoticeAcknowledged { get; set; }
    public DateTime PrivacyNoticeAcknowledgedAt { get; set; }
    public bool DailyUpdatesConsent { get; set; }
    public DateTime? DailyUpdatesConsentAt { get; set; }
    public bool? DailyUpdatesWithdrawn { get; set; }
    public bool PhotosVideosConsent { get; set; }
    public DateTime? PhotosVideosConsentAt { get; set; }
    public bool? PhotosVideosWithdrawn { get; set; }
    public bool ThirdPartySharingConsent { get; set; }
    public DateTime? ThirdPartySharingConsentAt { get; set; }
    public bool? ThirdPartySharingWithdrawn { get; set; }
    public bool LegitimateInterestConsent { get; set; }
    public DateTime LegitimateInterestConsentAt { get; set; }
    public bool TermsAccepted { get; set; }
    public DateTime TermsAcceptedAt { get; set; }

    public bool ConfirmAccuracyAndTruth { get; set; }

    public string Status { get; set; } = "Pending";
    public string? ApprovalNotes { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserId { get; set; }

    public DateTime? DataRetentionExpiryDate { get; set; }
    public bool DataRetentionCompleted { get; set; }

    public ICollection<CarerConsentAuditEntryEntity> ConsentAuditEntries { get; set; } = new List<CarerConsentAuditEntryEntity>();
}

public class CarerConsentAuditEntryEntity
{
    public int Id { get; set; }
    public int CarerOnboardingId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ConsentType { get; set; } = string.Empty;
    public bool Granted { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Reason { get; set; }

    public CarerOnboardingEntity CarerOnboarding { get; set; } = default!;
}
