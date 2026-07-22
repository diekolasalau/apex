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
    /// Interface for tutor onboarding operations
    /// GDPR compliant and safeguarding-aware
    /// </summary>
    public interface ITutorOnboardingService
    {
        Task<Response<TutorOnboardingData>> SaveOnboardingAsync(TutorOnboardingModel model);
        Task<TutorOnboardingData?> GetOnboardingByIdAsync(int id);
        Task<IEnumerable<TutorOnboardingData>> GetAllOnboardingsAsync();
        Task<IEnumerable<TutorOnboardingData>> GetPendingOnboardingsAsync();
        Task<bool> ApproveOnboardingAsync(int id, string adminNotes);
        Task<bool> DeclineOnboardingAsync(int id, string reason, string changedBy = "Admin");
        Task<bool> RequestReworkOnboardingAsync(int id, string reason, string changedBy = "Admin");
        Task<bool> UpdateOnboardingAsync(int id, TutorAdminUpdateModel model, string changedBy = "Admin");
        Task<bool> UpdateCoursePlanningAsync(int id, string? coursesToBeTaken, string? courseDuration, string changedBy = "Admin");
        Task<bool> UpdateCoursePlanningAsync(int id, string? coursesToBeTaken, string? daysOfWeek, string? hoursPerDay, string changedBy = "Admin");
        Task<bool> UpdateCoursePlanningAsync(int id, string? coursesToBeTaken, string? daysOfWeek, string? hoursPerDay, string? startTime, string? finishTime, string changedBy = "Admin");
        Task<AuditTrail> GetAuditTrailAsync(int tutorId);
    }

    /// <summary>
    /// Tutor onboarding form model with validation
    /// </summary>
    public class TutorOnboardingModel
    {
        // Personal Details
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

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        // Qualifications & References
        [Required(ErrorMessage = "Highest qualification is required")]
        [StringLength(200)]
        public string HighestQualification { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? OtherQualifications { get; set; }

        [Required(ErrorMessage = "Teaching experience is required")]
        [StringLength(1000)]
        public string TeachingExperience { get; set; } = string.Empty;

        [StringLength(500)]
        public string? CoursesToBeTaken { get; set; }

        [StringLength(120)]
        public string? AvailableDays { get; set; }

        [StringLength(100)]
        public string? CourseDuration { get; set; }

        [Required(ErrorMessage = "At least one reference is required")]
        [StringLength(500)]
        public string Reference1Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reference contact details are required")]
        [StringLength(200)]
        public string Reference1Contact { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reference2Name { get; set; }

        [StringLength(200)]
        public string? Reference2Contact { get; set; }

        // DBS Check
        [Required(ErrorMessage = "DBS check status is required")]
        public string DBSStatus { get; set; } = string.Empty;

        [StringLength(50)]
        public string? DBSCertificateNumber { get; set; }

        public DateTime? DBSIssueDate { get; set; }

        public DateTime? DBSExpiryDate { get; set; }

        public string? DBSCheckType { get; set; }

        // Right to Work
        [Required(ErrorMessage = "Right to work verification is required")]
        public string RightToWorkStatus { get; set; } = string.Empty;

        [StringLength(50)]
        public string? VisaType { get; set; }

        public DateTime? VisaExpiryDate { get; set; }

        [StringLength(50)]
        public string? PassportNumber { get; set; }

        // Safeguarding Training
        [Required(ErrorMessage = "Safeguarding training status is required")]
        public string SafeguardingTrainingStatus { get; set; } = string.Empty;

        public DateTime? SafeguardingTrainingDate { get; set; }

        public DateTime? SafeguardingTrainingExpiry { get; set; }

        [StringLength(100)]
        public string? TrainingProvider { get; set; }

        // Contract Details
        [Required(ErrorMessage = "Contract type is required")]
        public string ContractType { get; set; } = string.Empty;

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        [StringLength(1000)]
        public string? ContractTerms { get; set; }

        // Consent
        [Required(ErrorMessage = "You must consent to data processing")]
        public bool ConsentDataProcessing { get; set; }

        [Required(ErrorMessage = "You must consent to DBS verification")]
        public bool ConsentDBSCheck { get; set; }

        [Required(ErrorMessage = "You must consent to reference checks")]
        public bool ConsentReferences { get; set; }

        public bool ConsentMarketing { get; set; }

        // Privacy & Declaration
        [Required(ErrorMessage = "You must acknowledge the privacy notice")]
        public bool PrivacyNoticeAcknowledged { get; set; }

        [Required(ErrorMessage = "You must confirm the declaration")]
        public bool DeclarationConfirmed { get; set; }
    }

    /// <summary>
    /// Stored tutor onboarding data (database model)
    /// </summary>
    public class TutorOnboardingData
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
        public string? AvailableDays { get; set; }
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

        public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;
        public string? AdminNotes { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }
    }

    public class TutorAdminUpdateModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string HighestQualification { get; set; } = string.Empty;
        public string TeachingExperience { get; set; } = string.Empty;
        public string? CoursesToBeTaken { get; set; }
        public string? AvailableDays { get; set; }
        public string? CourseDuration { get; set; }
        public string DBSStatus { get; set; } = string.Empty;
        public string RightToWorkStatus { get; set; } = string.Empty;
        public string SafeguardingTrainingStatus { get; set; } = string.Empty;
        public string ContractType { get; set; } = string.Empty;
        public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;
        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// Onboarding status enumeration (shared)
    /// </summary>
    // Note: OnboardingStatus enum is defined in CarerOnboardingService.cs

    /// <summary>
    /// Consent type enumeration (shared)
    /// </summary>
    // Note: ConsentType enum is defined in CarerOnboardingService.cs

    /// <summary>
    /// Default implementation of tutor onboarding service
    /// </summary>
    public class TutorOnboardingService : ITutorOnboardingService
    {
        private readonly StudyMgtDbContext _dbContext;
        private readonly AppAuditLogService? _auditLog;

        public TutorOnboardingService(StudyMgtDbContext dbContext, AppAuditLogService? auditLog = null)
        {
            _dbContext = dbContext;
            _auditLog = auditLog;
        }

        public async Task<Response<TutorOnboardingData>> SaveOnboardingAsync(TutorOnboardingModel model)
        {
            try
            {
                var entity = new TutorOnboardingEntity
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = EnsureUtc(model.DateOfBirth),
                    Gender = model.Gender,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    HighestQualification = model.HighestQualification,
                    OtherQualifications = model.OtherQualifications,
                    TeachingExperience = model.TeachingExperience,
                    CoursesToBeTaken = model.CoursesToBeTaken,
                    CourseDuration = ComposeCourseDuration(model.AvailableDays, model.CourseDuration),
                    Reference1Name = model.Reference1Name,
                    Reference1Contact = model.Reference1Contact,
                    Reference2Name = model.Reference2Name,
                    Reference2Contact = model.Reference2Contact,
                    DBSStatus = model.DBSStatus,
                    DBSCertificateNumber = model.DBSCertificateNumber,
                    DBSIssueDate = EnsureUtc(model.DBSIssueDate),
                    DBSExpiryDate = EnsureUtc(model.DBSExpiryDate),
                    DBSCheckType = model.DBSCheckType,
                    RightToWorkStatus = model.RightToWorkStatus,
                    VisaType = model.VisaType,
                    VisaExpiryDate = EnsureUtc(model.VisaExpiryDate),
                    PassportNumber = model.PassportNumber,
                    SafeguardingTrainingStatus = model.SafeguardingTrainingStatus,
                    SafeguardingTrainingDate = EnsureUtc(model.SafeguardingTrainingDate),
                    SafeguardingTrainingExpiry = EnsureUtc(model.SafeguardingTrainingExpiry),
                    TrainingProvider = model.TrainingProvider,
                    ContractType = model.ContractType,
                    ContractStartDate = EnsureUtc(model.ContractStartDate),
                    ContractEndDate = EnsureUtc(model.ContractEndDate),
                    ContractTerms = model.ContractTerms,
                    ConsentDataProcessing = model.ConsentDataProcessing,
                    ConsentDBSCheck = model.ConsentDBSCheck,
                    ConsentReferences = model.ConsentReferences,
                    ConsentMarketing = model.ConsentMarketing,
                    PrivacyNoticeAcknowledged = model.PrivacyNoticeAcknowledged,
                    DeclarationConfirmed = model.DeclarationConfirmed,
                    Status = OnboardingStatus.Pending.ToString(),
                    SubmittedDate = DateTime.UtcNow
                };

                _dbContext.TutorOnboardings.Add(entity);
                await _dbContext.SaveChangesAsync();

                _dbContext.TutorOnboardingAudits.Add(new TutorOnboardingAuditEntity
                {
                    TutorOnboardingId = entity.Id,
                    Action = "Created",
                    ChangedBy = "System",
                    ChangedDate = DateTime.UtcNow,
                    Reason = "Initial submission"
                });

                await _dbContext.SaveChangesAsync();

                var data = MapTutor(entity);

                await LogAuditAsync(
                    eventType: "Business",
                    action: "TutorOnboardingSubmitted",
                    pagePath: "/tutor-onboarding",
                    actorRole: "Anonymous",
                    actorUsername: "anonymous",
                    entityType: "TutorOnboarding",
                    entityId: entity.Id.ToString(),
                    success: true,
                    details: $"Tutor onboarding submitted for {entity.FirstName} {entity.LastName}".Trim());

                return new Response<TutorOnboardingData>
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
                    action: "TutorOnboardingSubmissionFailed",
                    pagePath: "/tutor-onboarding",
                    actorRole: "Anonymous",
                    actorUsername: "anonymous",
                    entityType: "TutorOnboarding",
                    success: false,
                    details: ex.Message);

                return new Response<TutorOnboardingData>
                {
                    Success = false,
                    Message = "Failed to save onboarding",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<TutorOnboardingData?> GetOnboardingByIdAsync(int id)
        {
            var entity = await _dbContext.TutorOnboardings
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            return entity is null ? null : MapTutor(entity);
        }

        public async Task<IEnumerable<TutorOnboardingData>> GetAllOnboardingsAsync()
        {
            var entities = await _dbContext.TutorOnboardings
                .AsNoTracking()
                .OrderByDescending(o => o.SubmittedDate)
                .ToListAsync();

            return entities.Select(MapTutor);
        }

        public async Task<IEnumerable<TutorOnboardingData>> GetPendingOnboardingsAsync()
        {
            var entities = await _dbContext.TutorOnboardings
                .AsNoTracking()
                .Where(o => o.Status == "Pending")
                .OrderByDescending(o => o.SubmittedDate)
                .ToListAsync();

            return entities.Select(MapTutor);
        }

        public async Task<bool> ApproveOnboardingAsync(int id, string adminNotes)
        {
            var onboarding = await _dbContext.TutorOnboardings.FirstOrDefaultAsync(o => o.Id == id);
            if (onboarding == null)
            {
                return false;
            }

            onboarding.Status = OnboardingStatus.Approved.ToString();
            onboarding.AdminNotes = adminNotes;
            onboarding.ApprovedDate = DateTime.UtcNow;
            onboarding.ApprovedBy = "Admin"; // In real app, get from current user

            _dbContext.TutorOnboardingAudits.Add(new TutorOnboardingAuditEntity
            {
                TutorOnboardingId = id,
                Action = "Approved",
                ChangedBy = "Admin",
                ChangedDate = DateTime.UtcNow,
                Reason = adminNotes
            });

            await _dbContext.SaveChangesAsync();

            await LogAuditAsync(
                eventType: "Business",
                action: "TutorOnboardingApproved",
                pagePath: "/admin-review",
                actorRole: "CentreAdmin",
                actorUsername: "admin",
                entityType: "TutorOnboarding",
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
                eventAction: "TutorOnboardingDeclined");
        }

        public Task<bool> RequestReworkOnboardingAsync(int id, string reason, string changedBy = "Admin")
        {
            return ApplyReviewDecisionAsync(
                id,
                OnboardingStatus.RequestedChanges,
                reason,
                changedBy,
                auditAction: "ReworkRequested",
                eventAction: "TutorOnboardingReworkRequested");
        }

        public async Task<bool> UpdateOnboardingAsync(int id, TutorAdminUpdateModel model, string changedBy = "Admin")
        {
            var onboarding = await _dbContext.TutorOnboardings.FirstOrDefaultAsync(o => o.Id == id);
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
            onboarding.Email = model.Email?.Trim() ?? string.Empty;
            onboarding.Phone = model.Phone?.Trim() ?? string.Empty;
            onboarding.Address = string.IsNullOrWhiteSpace(model.Address)
                ? null
                : model.Address.Trim();
            onboarding.HighestQualification = model.HighestQualification?.Trim() ?? string.Empty;
            onboarding.TeachingExperience = model.TeachingExperience?.Trim() ?? string.Empty;
            onboarding.CoursesToBeTaken = string.IsNullOrWhiteSpace(model.CoursesToBeTaken)
                ? null
                : model.CoursesToBeTaken.Trim();
            onboarding.CourseDuration = ComposeCourseDuration(model.AvailableDays, model.CourseDuration);
            onboarding.DBSStatus = model.DBSStatus?.Trim() ?? string.Empty;
            onboarding.RightToWorkStatus = model.RightToWorkStatus?.Trim() ?? string.Empty;
            onboarding.SafeguardingTrainingStatus = model.SafeguardingTrainingStatus?.Trim() ?? string.Empty;
            onboarding.ContractType = model.ContractType?.Trim() ?? string.Empty;
            onboarding.Status = model.Status.ToString();
            onboarding.AdminNotes = string.IsNullOrWhiteSpace(model.AdminNotes)
                ? null
                : model.AdminNotes.Trim();

            _dbContext.TutorOnboardingAudits.Add(new TutorOnboardingAuditEntity
            {
                TutorOnboardingId = id,
                Action = "Updated",
                ChangedBy = changedBy,
                ChangedDate = DateTime.UtcNow,
                Reason = "Record updated by centre administrator"
            });

            await _dbContext.SaveChangesAsync();
            await LogAuditAsync(
                eventType: "Business",
                action: "TutorOnboardingUpdated",
                pagePath: "/admin-review",
                actorRole: "CentreAdmin",
                actorUsername: changedBy,
                entityType: "TutorOnboarding",
                entityId: id.ToString(),
                success: true,
                details: "Tutor onboarding record updated by centre administrator.");

            return true;
        }

        public async Task<bool> UpdateCoursePlanningAsync(int id, string? coursesToBeTaken, string? courseDuration, string changedBy = "Admin")
        {
            return await UpdateCoursePlanningAsync(id, coursesToBeTaken, null, courseDuration, null, null, changedBy);
        }

        public async Task<bool> UpdateCoursePlanningAsync(int id, string? coursesToBeTaken, string? daysOfWeek, string? hoursPerDay, string changedBy = "Admin")
        {
            return await UpdateCoursePlanningAsync(id, coursesToBeTaken, daysOfWeek, hoursPerDay, null, null, changedBy);
        }

        public async Task<bool> UpdateCoursePlanningAsync(int id, string? coursesToBeTaken, string? daysOfWeek, string? hoursPerDay, string? startTime, string? finishTime, string changedBy = "Admin")
        {
            var onboarding = await _dbContext.TutorOnboardings.FirstOrDefaultAsync(o => o.Id == id);
            if (onboarding == null)
            {
                return false;
            }

            onboarding.CoursesToBeTaken = string.IsNullOrWhiteSpace(coursesToBeTaken)
                ? null
                : coursesToBeTaken.Trim();

            onboarding.CourseDuration = ComposeCourseDuration(daysOfWeek, hoursPerDay, startTime, finishTime);

            _dbContext.TutorOnboardingAudits.Add(new TutorOnboardingAuditEntity
            {
                TutorOnboardingId = id,
                Action = "Updated",
                FieldChanged = "CoursesToBeTaken,CourseDuration",
                ChangedBy = changedBy,
                ChangedDate = DateTime.UtcNow,
                Reason = "Course planning updated during student assignment"
            });

            await _dbContext.SaveChangesAsync();
            await LogAuditAsync(
                eventType: "Business",
                action: "TutorCoursePlanningUpdated",
                pagePath: "/admin-review",
                actorRole: "CentreAdmin",
                actorUsername: changedBy,
                entityType: "TutorOnboarding",
                entityId: id.ToString(),
                success: true,
                details: "Tutor course planning fields updated by centre administrator.");

            return true;
        }

        private static string? ComposeCourseDuration(string? daysOfWeek, string? hoursPerDay, string? startTime = null, string? finishTime = null)
        {
            var normalizedDays = string.IsNullOrWhiteSpace(daysOfWeek) ? null : daysOfWeek.Trim();
            var normalizedHours = string.IsNullOrWhiteSpace(hoursPerDay) ? null : hoursPerDay.Trim();
            var normalizedStart = string.IsNullOrWhiteSpace(startTime) ? null : startTime.Trim();
            var normalizedFinish = string.IsNullOrWhiteSpace(finishTime) ? null : finishTime.Trim();
            var hasTimeWindow = !string.IsNullOrWhiteSpace(normalizedStart) && !string.IsNullOrWhiteSpace(normalizedFinish);

            if (string.IsNullOrWhiteSpace(normalizedDays) && string.IsNullOrWhiteSpace(normalizedHours) && !hasTimeWindow)
            {
                return null;
            }

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(normalizedDays))
            {
                parts.Add($"Days: {normalizedDays}");
            }

            if (hasTimeWindow)
            {
                parts.Add($"Time: {normalizedStart}-{normalizedFinish}");
            }

            if (!string.IsNullOrWhiteSpace(normalizedHours))
            {
                parts.Add($"Hours/Day: {normalizedHours}");
            }

            if (parts.Count == 0)
            {
                return null;
            }

            return string.Join("; ", parts);
        }

        private static string? ExtractAvailableDays(string? courseDuration)
        {
            if (string.IsNullOrWhiteSpace(courseDuration))
            {
                return null;
            }

            const string daysPrefix = "Days:";
            const string separator = "; Hours/Day:";
            var value = courseDuration.Trim();
            if (!value.StartsWith(daysPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var separatorIndex = value.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (separatorIndex < 0)
            {
                var onlyDays = value[daysPrefix.Length..].Trim();
                return string.IsNullOrWhiteSpace(onlyDays) ? null : onlyDays;
            }

            var days = value.Substring(daysPrefix.Length, separatorIndex - daysPrefix.Length).Trim();
            return string.IsNullOrWhiteSpace(days) ? null : days;
        }

        private static string? ExtractCourseDurationValue(string? courseDuration)
        {
            if (string.IsNullOrWhiteSpace(courseDuration))
            {
                return null;
            }

            const string separator = "; Hours/Day:";
            var value = courseDuration.Trim();
            var separatorIndex = value.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (separatorIndex < 0)
            {
                if (value.StartsWith("Days:", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return value;
            }

            var hours = value[(separatorIndex + separator.Length)..].Trim();
            return string.IsNullOrWhiteSpace(hours) ? null : hours;
        }

        public async Task<AuditTrail> GetAuditTrailAsync(int tutorId)
        {
            var latest = await _dbContext.TutorOnboardingAudits
                .AsNoTracking()
                .Where(a => a.TutorOnboardingId == tutorId)
                .OrderByDescending(a => a.ChangedDate)
                .FirstOrDefaultAsync();

            if (latest is null)
            {
                return new AuditTrail
                {
                    Id = 0,
                    StudentId = tutorId,
                    Action = "No Audit Entries",
                    ChangedBy = "System",
                    ChangedDate = DateTime.UtcNow,
                    Reason = "No audit entries were found"
                };
            }

            return new AuditTrail
            {
                Id = latest.Id,
                StudentId = tutorId,
                Action = latest.Action,
                FieldChanged = latest.FieldChanged,
                OldValue = latest.OldValue,
                NewValue = latest.NewValue,
                ChangedBy = latest.ChangedBy,
                ChangedDate = latest.ChangedDate,
                Reason = latest.Reason
            };
        }

        private static TutorOnboardingData MapTutor(TutorOnboardingEntity entity)
        {
            return new TutorOnboardingData
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                DateOfBirth = entity.DateOfBirth,
                Gender = entity.Gender,
                Email = entity.Email,
                Phone = entity.Phone,
                Address = entity.Address,
                HighestQualification = entity.HighestQualification,
                OtherQualifications = entity.OtherQualifications,
                TeachingExperience = entity.TeachingExperience,
                CoursesToBeTaken = entity.CoursesToBeTaken,
                AvailableDays = ExtractAvailableDays(entity.CourseDuration),
                CourseDuration = entity.CourseDuration,
                Reference1Name = entity.Reference1Name,
                Reference1Contact = entity.Reference1Contact,
                Reference2Name = entity.Reference2Name,
                Reference2Contact = entity.Reference2Contact,
                DBSStatus = entity.DBSStatus,
                DBSCertificateNumber = entity.DBSCertificateNumber,
                DBSIssueDate = entity.DBSIssueDate,
                DBSExpiryDate = entity.DBSExpiryDate,
                DBSCheckType = entity.DBSCheckType,
                RightToWorkStatus = entity.RightToWorkStatus,
                VisaType = entity.VisaType,
                VisaExpiryDate = entity.VisaExpiryDate,
                PassportNumber = entity.PassportNumber,
                SafeguardingTrainingStatus = entity.SafeguardingTrainingStatus,
                SafeguardingTrainingDate = entity.SafeguardingTrainingDate,
                SafeguardingTrainingExpiry = entity.SafeguardingTrainingExpiry,
                TrainingProvider = entity.TrainingProvider,
                ContractType = entity.ContractType,
                ContractStartDate = entity.ContractStartDate,
                ContractEndDate = entity.ContractEndDate,
                ContractTerms = entity.ContractTerms,
                ConsentDataProcessing = entity.ConsentDataProcessing,
                ConsentDBSCheck = entity.ConsentDBSCheck,
                ConsentReferences = entity.ConsentReferences,
                ConsentMarketing = entity.ConsentMarketing,
                PrivacyNoticeAcknowledged = entity.PrivacyNoticeAcknowledged,
                DeclarationConfirmed = entity.DeclarationConfirmed,
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

        private static DateTime? EnsureUtc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return EnsureUtc(value.Value);
        }

        private async Task<bool> ApplyReviewDecisionAsync(
            int id,
            OnboardingStatus status,
            string reason,
            string changedBy,
            string auditAction,
            string eventAction)
        {
            var onboarding = await _dbContext.TutorOnboardings.FirstOrDefaultAsync(o => o.Id == id);
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

            _dbContext.TutorOnboardingAudits.Add(new TutorOnboardingAuditEntity
            {
                TutorOnboardingId = id,
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
                entityType: "TutorOnboarding",
                entityId: id.ToString(),
                success: true,
                details: notes ?? string.Empty);

            return true;
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