using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudyMgt.Data;
using StudyMgt.Data.Entities;

namespace StudyMgt.Services
{
    /// <summary>
    /// Interface for student onboarding operations
    /// GDPR compliant and SEN-aware
    /// </summary>
    public interface IStudentOnboardingService
    {
        Task<Response<StudentOnboardingData>> SaveOnboardingAsync(StudentOnboardingModel model);
        Task<StudentOnboardingData?> GetOnboardingByIdAsync(int id);
        Task<IEnumerable<StudentOnboardingData>> GetAllOnboardingsAsync();
        Task<IEnumerable<StudentOnboardingData>> GetPendingOnboardingsAsync();
        Task<bool> ApproveOnboardingAsync(int id, string adminNotes);
        Task<bool> DeclineOnboardingAsync(int id, string reason, string changedBy = "Admin");
        Task<bool> RequestReworkOnboardingAsync(int id, string reason, string changedBy = "Admin");
        Task<bool> UpdateOnboardingAsync(int id, StudentAdminUpdateModel model, string changedBy = "Admin");
        Task<bool> AssignTutorAsync(int studentId, int tutorId, string tutorName);
        Task<AuditTrail> GetAuditTrailAsync(int studentId);
    }

    /// <summary>
    /// Student onboarding form model with validation
    /// </summary>
    public class StudentOnboardingModel
    {
        // Student Demographics
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [StringLength(50)]
        public string? Gender { get; set; }

        [StringLength(50)]
        public string? StudentId { get; set; }

        // Emergency Contact
        [Required(ErrorMessage = "Emergency contact name is required")]
        [StringLength(100)]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Emergency contact phone is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string? EmergencyContactEmail { get; set; }

        [Required(ErrorMessage = "Relationship to student is required")]
        public string RelationshipToStudent { get; set; } = string.Empty;

        // SEN Information
        [Required(ErrorMessage = "SEN indicators must be specified")]
        [StringLength(2000)]
        public string SENIndicators { get; set; } = string.Empty;

        [Required(ErrorMessage = "EHCP status is required")]
        public string EHCPStatus { get; set; } = string.Empty;

        public string? EHCPDocumentName { get; set; }

        [StringLength(2000)]
        public string? ILPSummary { get; set; }

        // Support Requirements
        public bool RequiresPhysicalAccommodation { get; set; }
        public bool RequiresHearingSupport { get; set; }
        public bool RequiresVisualSupport { get; set; }
        public bool RequiresCommunicationSupport { get; set; }
        public bool RequiresBehaviorSupport { get; set; }

        // Safeguarding & Health
        [StringLength(2000)]
        public string? SafeguardingNotes { get; set; }

        [StringLength(1000)]
        public string? MedicalInformation { get; set; }

        [StringLength(1000)]
        public string? RiskAssessmentNotes { get; set; }

        // Consent
        [Required(ErrorMessage = "You must consent to data sharing for educational support")]
        public bool ConsentDataSharing { get; set; }

        public bool ConsentPhotos { get; set; }

        [Required(ErrorMessage = "You must consent to email communication")]
        public bool ConsentEmailCommunication { get; set; }

        public bool ConsentSMSCommunication { get; set; }

        [Required(ErrorMessage = "Please select a preferred contact method")]
        public string PreferredContactMethod { get; set; } = string.Empty;

        // Privacy & Declaration
        [Required(ErrorMessage = "You must acknowledge the privacy notice")]
        public bool PrivacyNoticeAcknowledged { get; set; }

        [Required(ErrorMessage = "You must confirm the declaration")]
        public bool DeclarationConfirmed { get; set; }
    }

    /// <summary>
    /// Stored student onboarding data (database model)
    /// </summary>
    public class StudentOnboardingData
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? StudentId { get; set; }

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

        public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;
        public string? AdminNotes { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }
    }

    public class StudentAdminUpdateModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? StudentId { get; set; }
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string? EmergencyContactEmail { get; set; }
        public string RelationshipToStudent { get; set; } = string.Empty;
        public string SENIndicators { get; set; } = string.Empty;
        public string EHCPStatus { get; set; } = string.Empty;
        public string? AssignedTutorName { get; set; }
        public string PreferredContactMethod { get; set; } = string.Empty;
        public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;
        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// Consent status tracking
    /// </summary>
    public class ConsentRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public ConsentType ConsentType { get; set; }
        public bool IsConsented { get; set; }
        public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Audit trail for compliance and security
    /// </summary>
    public class AuditTrail
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? FieldChanged { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Generic response wrapper for API operations
    /// </summary>
    public class Response<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Onboarding status enumeration (shared with Carer Onboarding)
    /// </summary>
    public enum StudentOnboardingStatus
    {
        Pending,
        UnderReview,
        Approved,
        RequestedChanges,
        Rejected
    }

    /// <summary>
    /// Consent type enumeration (shared with Carer Onboarding)
    /// </summary>
    public enum StudentConsentType
    {
        DataSharing,
        Photos,
        EmailCommunication,
        SMSCommunication,
        Privacy,
        Declaration
    }

    /// <summary>
    /// Default implementation of student onboarding service
    /// </summary>
    public class StudentOnboardingService : IStudentOnboardingService
    {
        private readonly StudyMgtDbContext _dbContext;
        private readonly AppAuditLogService? _auditLog;

        public StudentOnboardingService(StudyMgtDbContext dbContext, AppAuditLogService? auditLog = null)
        {
            _dbContext = dbContext;
            _auditLog = auditLog;
        }

        public async Task<Response<StudentOnboardingData>> SaveOnboardingAsync(StudentOnboardingModel model)
        {
            try
            {
                var validationErrors = ValidateModel(model);
                if (validationErrors.Count > 0)
                {
                    return new Response<StudentOnboardingData>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = validationErrors
                    };
                }

                var entity = new StudentOnboardingEntity
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    DateOfBirth = EnsureUtc(model.DateOfBirth),
                    Gender = model.Gender?.Trim(),
                    StudentIdentifier = model.StudentId?.Trim(),
                    EmergencyContactName = model.EmergencyContactName.Trim(),
                    EmergencyContactPhone = model.EmergencyContactPhone.Trim(),
                    EmergencyContactEmail = string.IsNullOrWhiteSpace(model.EmergencyContactEmail) ? null : model.EmergencyContactEmail.Trim(),
                    RelationshipToStudent = model.RelationshipToStudent.Trim(),
                    SENIndicators = model.SENIndicators.Trim(),
                    EHCPStatus = model.EHCPStatus.Trim(),
                    EHCPDocumentPath = null, // File upload not implemented yet
                    ILPSummary = model.ILPSummary?.Trim(),
                    RequiresPhysicalAccommodation = model.RequiresPhysicalAccommodation,
                    RequiresHearingSupport = model.RequiresHearingSupport,
                    RequiresVisualSupport = model.RequiresVisualSupport,
                    RequiresCommunicationSupport = model.RequiresCommunicationSupport,
                    RequiresBehaviorSupport = model.RequiresBehaviorSupport,
                    SafeguardingNotes = model.SafeguardingNotes?.Trim(),
                    MedicalInformation = model.MedicalInformation?.Trim(),
                    RiskAssessmentNotes = model.RiskAssessmentNotes?.Trim(),
                    ConsentDataSharing = model.ConsentDataSharing,
                    ConsentPhotos = model.ConsentPhotos,
                    ConsentEmailCommunication = model.ConsentEmailCommunication,
                    ConsentSMSCommunication = model.ConsentSMSCommunication,
                    PreferredContactMethod = model.PreferredContactMethod.Trim(),
                    PrivacyNoticeAcknowledged = model.PrivacyNoticeAcknowledged,
                    DeclarationConfirmed = model.DeclarationConfirmed,
                    Status = OnboardingStatus.Pending.ToString(),
                    SubmittedDate = DateTime.UtcNow
                };

                _dbContext.StudentOnboardings.Add(entity);
                await _dbContext.SaveChangesAsync();

                _dbContext.StudentOnboardingAudits.Add(new StudentOnboardingAuditEntity
                {
                    StudentOnboardingId = entity.Id,
                    Action = "Created",
                    ChangedBy = "System",
                    ChangedDate = DateTime.UtcNow,
                    Reason = "Initial submission"
                });

                _dbContext.StudentConsentRecords.AddRange(
                    new StudentConsentRecordEntity
                    {
                        StudentOnboardingId = entity.Id,
                        ConsentType = StudentConsentType.DataSharing.ToString(),
                        IsConsented = model.ConsentDataSharing,
                        RecordedDate = DateTime.UtcNow
                    },
                    new StudentConsentRecordEntity
                    {
                        StudentOnboardingId = entity.Id,
                        ConsentType = StudentConsentType.Photos.ToString(),
                        IsConsented = model.ConsentPhotos,
                        RecordedDate = DateTime.UtcNow
                    },
                    new StudentConsentRecordEntity
                    {
                        StudentOnboardingId = entity.Id,
                        ConsentType = StudentConsentType.EmailCommunication.ToString(),
                        IsConsented = model.ConsentEmailCommunication,
                        RecordedDate = DateTime.UtcNow
                    },
                    new StudentConsentRecordEntity
                    {
                        StudentOnboardingId = entity.Id,
                        ConsentType = StudentConsentType.SMSCommunication.ToString(),
                        IsConsented = model.ConsentSMSCommunication,
                        RecordedDate = DateTime.UtcNow
                    },
                    new StudentConsentRecordEntity
                    {
                        StudentOnboardingId = entity.Id,
                        ConsentType = StudentConsentType.Privacy.ToString(),
                        IsConsented = model.PrivacyNoticeAcknowledged,
                        RecordedDate = DateTime.UtcNow
                    },
                    new StudentConsentRecordEntity
                    {
                        StudentOnboardingId = entity.Id,
                        ConsentType = StudentConsentType.Declaration.ToString(),
                        IsConsented = model.DeclarationConfirmed,
                        RecordedDate = DateTime.UtcNow
                    });

                await _dbContext.SaveChangesAsync();

                var data = MapStudent(entity);

                await LogAuditAsync(
                    eventType: "Business",
                    action: "StudentOnboardingSubmitted",
                    pagePath: "/student-onboarding",
                    actorRole: "Anonymous",
                    actorUsername: "anonymous",
                    entityType: "StudentOnboarding",
                    entityId: entity.Id.ToString(),
                    success: true,
                    details: $"Student onboarding submitted for {entity.FirstName} {entity.LastName}".Trim());

                return new Response<StudentOnboardingData>
                {
                    Success = true,
                    Message = "Onboarding submitted successfully",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                await LogAuditAsync(
                    eventType: "Business",
                    action: "StudentOnboardingSubmissionFailed",
                    pagePath: "/student-onboarding",
                    actorRole: "Anonymous",
                    actorUsername: "anonymous",
                    entityType: "StudentOnboarding",
                    success: false,
                    details: ex.Message);

                return new Response<StudentOnboardingData>
                {
                    Success = false,
                    Message = "Failed to save onboarding",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<StudentOnboardingData?> GetOnboardingByIdAsync(int id)
        {
            var entity = await _dbContext.StudentOnboardings
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            return entity is null ? null : MapStudent(entity);
        }

        public async Task<IEnumerable<StudentOnboardingData>> GetAllOnboardingsAsync()
        {
            var entities = await _dbContext.StudentOnboardings
                .AsNoTracking()
                .OrderByDescending(o => o.SubmittedDate)
                .ToListAsync();

            return entities.Select(MapStudent);
        }

        public async Task<IEnumerable<StudentOnboardingData>> GetPendingOnboardingsAsync()
        {
            var entities = await _dbContext.StudentOnboardings
                .AsNoTracking()
                .Where(o => o.Status == "Pending")
                .OrderByDescending(o => o.SubmittedDate)
                .ToListAsync();

            return entities.Select(MapStudent);
        }

        public async Task<bool> ApproveOnboardingAsync(int id, string adminNotes)
        {
            var onboarding = await _dbContext.StudentOnboardings.FirstOrDefaultAsync(o => o.Id == id);
            if (onboarding == null)
            {
                return false;
            }

            onboarding.Status = OnboardingStatus.Approved.ToString();
            onboarding.AdminNotes = adminNotes;
            onboarding.ApprovedDate = DateTime.UtcNow;
            onboarding.ApprovedBy = "Admin"; // In real app, get from current user

            _dbContext.StudentOnboardingAudits.Add(new StudentOnboardingAuditEntity
            {
                StudentOnboardingId = id,
                Action = "Approved",
                ChangedBy = "Admin",
                ChangedDate = DateTime.UtcNow,
                Reason = adminNotes
            });

            await _dbContext.SaveChangesAsync();

            await LogAuditAsync(
                eventType: "Business",
                action: "StudentOnboardingApproved",
                pagePath: "/admin-review",
                actorRole: "CentreAdmin",
                actorUsername: "admin",
                entityType: "StudentOnboarding",
                entityId: id.ToString(),
                success: true,
                details: adminNotes);

            return true;
        }

        public Task<bool> DeclineOnboardingAsync(int id, string reason, string changedBy = "Admin")
        {
            return ApplyReviewDecisionAsync(
                id,
                OnboardingStatus.Rejected,
                reason,
                changedBy,
                auditAction: "Declined",
                eventAction: "StudentOnboardingDeclined");
        }

        public Task<bool> RequestReworkOnboardingAsync(int id, string reason, string changedBy = "Admin")
        {
            return ApplyReviewDecisionAsync(
                id,
                OnboardingStatus.RequestedChanges,
                reason,
                changedBy,
                auditAction: "ReworkRequested",
                eventAction: "StudentOnboardingReworkRequested");
        }

        public async Task<bool> UpdateOnboardingAsync(int id, StudentAdminUpdateModel model, string changedBy = "Admin")
        {
            var onboarding = await _dbContext.StudentOnboardings.FirstOrDefaultAsync(o => o.Id == id);
            if (onboarding == null)
            {
                return false;
            }

            onboarding.FirstName = model.FirstName?.Trim() ?? string.Empty;
            onboarding.LastName = model.LastName?.Trim() ?? string.Empty;
            onboarding.DateOfBirth = EnsureUtc(model.DateOfBirth);
            onboarding.Gender = string.IsNullOrWhiteSpace(model.Gender)
                ? null
                : model.Gender.Trim();
            onboarding.StudentIdentifier = string.IsNullOrWhiteSpace(model.StudentId)
                ? null
                : model.StudentId.Trim();
            onboarding.EmergencyContactName = model.EmergencyContactName?.Trim() ?? string.Empty;
            onboarding.EmergencyContactPhone = model.EmergencyContactPhone?.Trim() ?? string.Empty;
            onboarding.EmergencyContactEmail = string.IsNullOrWhiteSpace(model.EmergencyContactEmail)
                ? null
                : model.EmergencyContactEmail.Trim();
            onboarding.RelationshipToStudent = model.RelationshipToStudent?.Trim() ?? string.Empty;
            onboarding.SENIndicators = model.SENIndicators?.Trim() ?? string.Empty;
            onboarding.EHCPStatus = model.EHCPStatus?.Trim() ?? string.Empty;
            onboarding.AssignedTutorName = string.IsNullOrWhiteSpace(model.AssignedTutorName)
                ? null
                : model.AssignedTutorName.Trim();
            onboarding.PreferredContactMethod = model.PreferredContactMethod?.Trim() ?? string.Empty;
            onboarding.Status = model.Status.ToString();
            onboarding.AdminNotes = string.IsNullOrWhiteSpace(model.AdminNotes)
                ? null
                : model.AdminNotes.Trim();

            _dbContext.StudentOnboardingAudits.Add(new StudentOnboardingAuditEntity
            {
                StudentOnboardingId = id,
                Action = "Updated",
                ChangedBy = changedBy,
                ChangedDate = DateTime.UtcNow,
                Reason = "Record updated by centre administrator"
            });

            await _dbContext.SaveChangesAsync();
            await LogAuditAsync(
                eventType: "Business",
                action: "StudentOnboardingUpdated",
                pagePath: "/admin-review",
                actorRole: "CentreAdmin",
                actorUsername: changedBy,
                entityType: "StudentOnboarding",
                entityId: id.ToString(),
                success: true,
                details: "Student onboarding record updated by centre administrator.");

            return true;
        }

        public async Task<bool> AssignTutorAsync(int studentId, int tutorId, string tutorName)
        {
            var onboarding = await _dbContext.StudentOnboardings.FirstOrDefaultAsync(o => o.Id == studentId);
            if (onboarding == null)
            {
                return false;
            }

            var tutor = await _dbContext.TutorOnboardings.FirstOrDefaultAsync(t => t.Id == tutorId);
            if (tutor == null)
            {
                return false;
            }

            // Assignment is only allowed between approved student and approved tutor.
            if (!string.Equals(onboarding.Status, OnboardingStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.Equals(tutor.Status, OnboardingStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var previousTutor = onboarding.AssignedTutorName;
            var assignedTutorName = string.IsNullOrWhiteSpace(tutorName)
                ? $"{tutor.FirstName} {tutor.LastName}".Trim()
                : tutorName;

            var assignmentAlreadyExists = await _dbContext.StudentTutorAssignments
                .AnyAsync(a => a.StudentOnboardingId == studentId && a.TutorOnboardingId == tutorId);

            if (assignmentAlreadyExists)
            {
                return true;
            }

            _dbContext.StudentTutorAssignments.Add(new StudentTutorAssignmentEntity
            {
                StudentOnboardingId = studentId,
                TutorOnboardingId = tutorId,
                TutorName = assignedTutorName,
                AssignedDateUtc = DateTime.UtcNow
            });

            var allTutorNames = await _dbContext.StudentTutorAssignments
                .Where(a => a.StudentOnboardingId == studentId)
                .Select(a => a.TutorName)
                .ToListAsync();

            allTutorNames.Add(assignedTutorName);

            onboarding.AssignedTutorId = tutorId;
            onboarding.AssignedTutorName = string.Join(", ", allTutorNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n));
            onboarding.AssignedDate = DateTime.UtcNow;

            _dbContext.StudentOnboardingAudits.Add(new StudentOnboardingAuditEntity
            {
                StudentOnboardingId = studentId,
                Action = "Tutor Assigned",
                ChangedBy = "Admin",
                ChangedDate = DateTime.UtcNow,
                FieldChanged = "AssignedTutorName",
                OldValue = previousTutor,
                NewValue = onboarding.AssignedTutorName,
                Reason = "Assigned additional tutor to student"
            });

            await _dbContext.SaveChangesAsync();

            await LogAuditAsync(
                eventType: "Business",
                action: "TutorAssignedToStudent",
                pagePath: "/admin-review",
                actorRole: "CentreAdmin",
                actorUsername: "admin",
                entityType: "StudentOnboarding",
                entityId: studentId.ToString(),
                success: true,
                details: $"Assigned tutor {assignedTutorName} (ID: {tutorId}) to student ID {studentId}. Current tutors: {onboarding.AssignedTutorName}.");

            return true;
        }

        public async Task<AuditTrail> GetAuditTrailAsync(int studentId)
        {
            var latest = await _dbContext.StudentOnboardingAudits
                .AsNoTracking()
                .Where(a => a.StudentOnboardingId == studentId)
                .OrderByDescending(a => a.ChangedDate)
                .FirstOrDefaultAsync();

            if (latest is null)
            {
                return new AuditTrail
                {
                    Id = 0,
                    StudentId = studentId,
                    Action = "No Audit Entries",
                    ChangedBy = "System",
                    ChangedDate = DateTime.UtcNow,
                    Reason = "No audit entries were found"
                };
            }

            return new AuditTrail
            {
                Id = latest.Id,
                StudentId = studentId,
                Action = latest.Action,
                FieldChanged = latest.FieldChanged,
                OldValue = latest.OldValue,
                NewValue = latest.NewValue,
                ChangedBy = latest.ChangedBy,
                ChangedDate = latest.ChangedDate,
                Reason = latest.Reason
            };
        }

        private static StudentOnboardingData MapStudent(StudentOnboardingEntity entity)
        {
            return new StudentOnboardingData
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                DateOfBirth = entity.DateOfBirth,
                Gender = entity.Gender,
                StudentId = entity.StudentIdentifier,
                EmergencyContactName = entity.EmergencyContactName,
                EmergencyContactPhone = entity.EmergencyContactPhone,
                EmergencyContactEmail = entity.EmergencyContactEmail,
                RelationshipToStudent = entity.RelationshipToStudent,
                SENIndicators = entity.SENIndicators,
                EHCPStatus = entity.EHCPStatus,
                EHCPDocumentPath = entity.EHCPDocumentPath,
                ILPSummary = entity.ILPSummary,
                RequiresPhysicalAccommodation = entity.RequiresPhysicalAccommodation,
                RequiresHearingSupport = entity.RequiresHearingSupport,
                RequiresVisualSupport = entity.RequiresVisualSupport,
                RequiresCommunicationSupport = entity.RequiresCommunicationSupport,
                RequiresBehaviorSupport = entity.RequiresBehaviorSupport,
                SafeguardingNotes = entity.SafeguardingNotes,
                MedicalInformation = entity.MedicalInformation,
                RiskAssessmentNotes = entity.RiskAssessmentNotes,
                ConsentDataSharing = entity.ConsentDataSharing,
                ConsentPhotos = entity.ConsentPhotos,
                ConsentEmailCommunication = entity.ConsentEmailCommunication,
                ConsentSMSCommunication = entity.ConsentSMSCommunication,
                PreferredContactMethod = entity.PreferredContactMethod,
                PrivacyNoticeAcknowledged = entity.PrivacyNoticeAcknowledged,
                DeclarationConfirmed = entity.DeclarationConfirmed,
                AssignedTutorId = entity.AssignedTutorId,
                AssignedTutorName = entity.AssignedTutorName,
                AssignedDate = entity.AssignedDate,
                Status = ParseStatus(entity.Status),
                AdminNotes = entity.AdminNotes,
                SubmittedDate = entity.SubmittedDate,
                ApprovedDate = entity.ApprovedDate,
                ApprovedBy = entity.ApprovedBy
            };
        }

        private static OnboardingStatus ParseStatus(string? status)
        {
            return Enum.TryParse<OnboardingStatus>(status, true, out var parsed)
                ? parsed
                : OnboardingStatus.Pending;
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            if (value == default)
            {
                return value;
            }

            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private async Task<bool> ApplyReviewDecisionAsync(
            int id,
            OnboardingStatus status,
            string reason,
            string changedBy,
            string auditAction,
            string eventAction)
        {
            var onboarding = await _dbContext.StudentOnboardings.FirstOrDefaultAsync(o => o.Id == id);
            if (onboarding == null)
            {
                return false;
            }

            var notes = string.IsNullOrWhiteSpace(reason)
                ? null
                : reason.Trim();

            var actor = string.IsNullOrWhiteSpace(changedBy)
                ? "Admin"
                : changedBy.Trim();

            onboarding.Status = status.ToString();
            onboarding.AdminNotes = notes;
            onboarding.ApprovedDate = null;
            onboarding.ApprovedBy = null;

            _dbContext.StudentOnboardingAudits.Add(new StudentOnboardingAuditEntity
            {
                StudentOnboardingId = id,
                Action = auditAction,
                ChangedBy = actor,
                ChangedDate = DateTime.UtcNow,
                Reason = notes
            });

            await _dbContext.SaveChangesAsync();

            await LogAuditAsync(
                eventType: "Business",
                action: eventAction,
                pagePath: "/admin-review",
                actorRole: "CentreAdmin",
                actorUsername: actor,
                entityType: "StudentOnboarding",
                entityId: id.ToString(),
                success: true,
                details: notes ?? string.Empty);

            return true;
        }

        private static List<string> ValidateModel(StudentOnboardingModel model)
        {
            var errors = new List<string>();

            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            var modelIsValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            if (!modelIsValid)
            {
                errors.AddRange(validationResults
                    .Select(v => v.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Select(m => m!));
            }

            if (model.DateOfBirth == default)
            {
                errors.Add("Date of birth is required.");
            }
            else if (model.DateOfBirth.Date > DateTime.Today)
            {
                errors.Add("Date of birth cannot be in the future.");
            }

            if (!model.PrivacyNoticeAcknowledged)
            {
                errors.Add("Privacy notice acknowledgement is required.");
            }

            if (!model.ConsentDataSharing)
            {
                errors.Add("Data sharing consent is required.");
            }

            if (!model.ConsentEmailCommunication)
            {
                errors.Add("Email communication consent is required.");
            }

            if (!model.DeclarationConfirmed)
            {
                errors.Add("Declaration confirmation is required.");
            }

            return errors.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private Task LogAuditAsync(
            string eventType,
            string action,
            string? pagePath = null,
            string? actorRole = null,
            string? actorUsername = null,
            string? entityType = null,
            string? entityId = null,
            bool success = true,
            string? details = null)
        {
            if (_auditLog is null)
            {
                return Task.CompletedTask;
            }

            return _auditLog.LogAsync(eventType, action, pagePath, actorRole, actorUsername, entityType, entityId, success, details);
        }
    }
}
