namespace StudyMgt.Data.Entities;

public class TutorOnboardingEntity
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }

    public string HighestQualification { get; set; } = string.Empty;
    public string? OtherQualifications { get; set; }
    public string TeachingExperience { get; set; } = string.Empty;
    public string? CoursesToBeTaken { get; set; }
    public string? CourseDuration { get; set; }
    public string Reference1Name { get; set; } = string.Empty;
    public string Reference1Contact { get; set; } = string.Empty;
    public string? Reference2Name { get; set; }
    public string? Reference2Contact { get; set; }

    public string DBSStatus { get; set; } = string.Empty;
    public string? DBSCertificateNumber { get; set; }
    public DateTime? DBSIssueDate { get; set; }
    public DateTime? DBSExpiryDate { get; set; }
    public string? DBSCheckType { get; set; }

    public string RightToWorkStatus { get; set; } = string.Empty;
    public string? VisaType { get; set; }
    public DateTime? VisaExpiryDate { get; set; }
    public string? PassportNumber { get; set; }

    public string SafeguardingTrainingStatus { get; set; } = string.Empty;
    public DateTime? SafeguardingTrainingDate { get; set; }
    public DateTime? SafeguardingTrainingExpiry { get; set; }
    public string? TrainingProvider { get; set; }

    public string ContractType { get; set; } = string.Empty;
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public string? ContractTerms { get; set; }

    public bool ConsentDataProcessing { get; set; }
    public bool ConsentDBSCheck { get; set; }
    public bool ConsentReferences { get; set; }
    public bool ConsentMarketing { get; set; }

    public bool PrivacyNoticeAcknowledged { get; set; }
    public bool DeclarationConfirmed { get; set; }

    public string Status { get; set; } = "Pending";
    public string? AdminNotes { get; set; }

    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedBy { get; set; }

    public ICollection<TutorOnboardingAuditEntity> Audits { get; set; } = new List<TutorOnboardingAuditEntity>();
}

public class TutorOnboardingAuditEntity
{
    public int Id { get; set; }
    public int TutorOnboardingId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? FieldChanged { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }

    public TutorOnboardingEntity TutorOnboarding { get; set; } = default!;
}
