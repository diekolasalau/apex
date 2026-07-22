namespace StudyMgt.Data.Entities;

public class StudentOnboardingEntity
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? StudentIdentifier { get; set; }

    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string? EmergencyContactEmail { get; set; }
    public string RelationshipToStudent { get; set; } = string.Empty;

    public string SENIndicators { get; set; } = string.Empty;
    public string EHCPStatus { get; set; } = string.Empty;
    public string? EHCPDocumentPath { get; set; }
    public string? ILPSummary { get; set; }

    public bool RequiresPhysicalAccommodation { get; set; }
    public bool RequiresHearingSupport { get; set; }
    public bool RequiresVisualSupport { get; set; }
    public bool RequiresCommunicationSupport { get; set; }
    public bool RequiresBehaviorSupport { get; set; }

    public string? SafeguardingNotes { get; set; }
    public string? MedicalInformation { get; set; }
    public string? RiskAssessmentNotes { get; set; }

    public bool ConsentDataSharing { get; set; }
    public bool ConsentPhotos { get; set; }
    public bool ConsentEmailCommunication { get; set; }
    public bool ConsentSMSCommunication { get; set; }
    public string PreferredContactMethod { get; set; } = string.Empty;

    public bool PrivacyNoticeAcknowledged { get; set; }
    public bool DeclarationConfirmed { get; set; }

    public int? AssignedTutorId { get; set; }
    public string? AssignedTutorName { get; set; }
    public DateTime? AssignedDate { get; set; }

    public string Status { get; set; } = "Pending";
    public string? AdminNotes { get; set; }

    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }

    public ICollection<StudentOnboardingAuditEntity> Audits { get; set; } = new List<StudentOnboardingAuditEntity>();
    public ICollection<StudentConsentRecordEntity> ConsentRecords { get; set; } = new List<StudentConsentRecordEntity>();
    public ICollection<StudentTutorAssignmentEntity> TutorAssignments { get; set; } = new List<StudentTutorAssignmentEntity>();
}
