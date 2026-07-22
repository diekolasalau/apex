using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using StudyMgt.Data;
using StudyMgt.Data.Entities;

namespace StudyMgt.Services
{
    /// <summary>
    /// Service for managing carer onboarding and GDPR compliance
    /// Implements UK GDPR, Data Protection Act 2018, and relevant safeguarding requirements
    /// </summary>
    public interface ICarerOnboardingService
    {
        Task<CarerOnboardingResult> SaveOnboardingAsync(CarerOnboardingData data);
        Task<CarerOnboardingData?> GetCarerByIdAsync(string carerId);
        Task<IEnumerable<CarerOnboardingData>> GetAllOnboardingsAsync();
        Task<IEnumerable<CarerOnboardingData>> GetPendingOnboardingsAsync();
        Task<bool> ApproveOnboardingAsync(string carerId, string adminNotes);
        Task<bool> UpdateOnboardingAsync(string carerId, CarerAdminUpdateModel model, string changedBy = "Admin");
        Task<bool> UpdateConsentAsync(string carerId, ConsentUpdate consent);
        Task<bool> DeleteCarerDataAsync(string carerId, string reason);
        Task<ConsentAuditLog> GetConsentHistoryAsync(string carerId);
    }

    /// <summary>
    /// Core model for carer onboarding - contains all data collected during registration
    /// </summary>
    public class CarerOnboardingData
    {
        public string? CarerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByUserId { get; set; } // For audit trail

        // Personal Information
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        // Student Information
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? Relationship { get; set; }
        public DateTime StudentDateOfBirth { get; set; }
        public string? EHCPStatus { get; set; }

        // Parental Authority Declaration
        public bool HasParentalResponsibility { get; set; }
        public bool NoRestrictiveOrders { get; set; }

        // Communication
        public string? PreferredContactMethod { get; set; }
        public string? MedicalAndAccessibilityInfo { get; set; }

        // Emergency Contact
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }

        // Consent Records (for audit trail compliance)
        public ConsentStatus ConsentsProvided { get; set; } = new();

        // Authorization
        public bool ConfirmAccuracyAndTruth { get; set; }

        // Status
        public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;
        public string? ApprovalNotes { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByUserId { get; set; }

        // Data Retention
        public DateTime? DataRetentionExpiryDate { get; set; } // Set to 6 years from enrollment end
        public bool DataRetentionCompleted { get; set; }
    }

    /// <summary>
    /// Records all consent status at time of submission
    /// Enables withdrawal of specific consents independently
    /// </summary>
    public class ConsentStatus
    {
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
    }

    /// <summary>
    /// Tracks consent changes for GDPR audit requirements
    /// </summary>
    public class ConsentUpdate
    {
        public string? CarerId { get; set; }
        public ConsentType ConsentType { get; set; }
        public bool IsGranting { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }
    }

    public enum ConsentType
    {
        DailyUpdates,
        PhotosVideos,
        ThirdPartySharing,
        LegitimateInterest
    }

    /// <summary>
    /// Audit log entry for tracking all consent changes
    /// Required for GDPR compliance and accountability
    /// </summary>
    public class ConsentAuditLog
    {
        public string? CarerId { get; set; }
        public List<ConsentAuditEntry> Entries { get; set; } = new();
    }

    public class ConsentAuditEntry
    {
        public DateTime Timestamp { get; set; }
        public ConsentType ConsentType { get; set; }
        public bool Granted { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Reason { get; set; }
    }

    public enum OnboardingStatus
    {
        Pending,        // Awaiting admin review
        RequestedChanges, // Returned for correction and resubmission
        Approved,       // Admin has verified identity
        Rejected,       // Identity verification failed
        Suspended,      // Safeguarding issue flagged
        Completed       // Fully onboarded
    }

    public class CarerOnboardingResult
    {
        public bool Success { get; set; }
        public string? CarerId { get; set; }
        public string? Message { get; set; }
        public OnboardingStatus Status { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class CarerAdminUpdateModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? Relationship { get; set; }
        public string? EHCPStatus { get; set; }
        public string? PreferredContactMethod { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;
        public string? ApprovalNotes { get; set; }
    }

    /// <summary>
    /// GDPR Data Access Request - Subject Access Request (SAR) support
    /// </summary>
    public class DataAccessRequest
    {
        public string? RequestId { get; set; }
        public string? CarerId { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? RequestedByEmail { get; set; }
        public DataAccessStatus Status { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? DataExportPath { get; set; }
    }

    public enum DataAccessStatus
    {
        Pending,
        InProgress,
        Completed,
        Denied,
        Expired
    }

    /// <summary>
    /// Privacy & Compliance Documentation
    /// UK GDPR Article 13 - Information to be provided to the data subject
    /// </summary>
    public class PrivacyNotice
    {
        public const string Title = "Privacy Notice for Parent/Guardian Onboarding";
        
        public const string DataController = "Study Management Ltd";
        public const string DataControllerEmail = "privacy@studymgt.com";
        public const string DataControllerAddress = "Your School Address";
        
        public const string LawfulBasis = "UK GDPR Article 6(1)(e) - Necessary for public task (education provision), and Article 6(1)(a) - Explicit consent for specific processing as indicated below";
        
        public const string Purpose = "To manage student enrollment, provide educational services, ensure student safeguarding, enable emergency contact, and comply with statutory obligations under the Education Act 1996 and Children Act 1989/2004";
        
        public const string Recipients = "School staff, authorized educational professionals, healthcare providers (with consent), local education authority (where statutory), and safeguarding partners as required by law";
        
        public const string RetentionPeriod = "Data retained for duration of student enrollment plus 6 years afterwards, or longer if required by statute. Sensitive safeguarding data may be retained longer in accordance with statutory guidance";
        
        public const string RightsInfo = @"Under UK GDPR, you have the right to:
- Access your personal data (Article 15)
- Rectify inaccurate data (Article 16)
- Restrict processing (Article 18)
- Withdraw consent at any time (Article 7)
- Data portability (Article 20)
- Lodge a complaint with the ICO (Article 77)

To exercise these rights, contact privacy@studymgt.com

Note: Some requests may be limited by safeguarding obligations and Children Act statutory requirements.";
    }

    /// <summary>
    /// Default implementation of carer onboarding service
    /// </summary>
    public class CarerOnboardingService : ICarerOnboardingService
    {
        private readonly StudyMgtDbContext _dbContext;
        private readonly AppAuditLogService? _auditLog;

        public CarerOnboardingService(StudyMgtDbContext dbContext, AppAuditLogService? auditLog = null)
        {
            _dbContext = dbContext;
            _auditLog = auditLog;
        }

        public async Task<CarerOnboardingResult> SaveOnboardingAsync(CarerOnboardingData data)
        {
            try
            {
                var carerId = await GenerateNextCarerIdAsync();
                var now = DateTime.UtcNow;

                data.CarerId = carerId;
                data.CreatedAt = now;
                data.Status = OnboardingStatus.Pending;

                // Set consent timestamps
                if (data.ConsentsProvided.PrivacyNoticeAcknowledged)
                    data.ConsentsProvided.PrivacyNoticeAcknowledgedAt = DateTime.UtcNow;
                if (data.ConsentsProvided.DailyUpdatesConsent)
                    data.ConsentsProvided.DailyUpdatesConsentAt = DateTime.UtcNow;
                if (data.ConsentsProvided.PhotosVideosConsent)
                    data.ConsentsProvided.PhotosVideosConsentAt = DateTime.UtcNow;
                if (data.ConsentsProvided.ThirdPartySharingConsent)
                    data.ConsentsProvided.ThirdPartySharingConsentAt = DateTime.UtcNow;
                if (data.ConsentsProvided.LegitimateInterestConsent)
                    data.ConsentsProvided.LegitimateInterestConsentAt = DateTime.UtcNow;
                if (data.ConsentsProvided.TermsAccepted)
                    data.ConsentsProvided.TermsAcceptedAt = DateTime.UtcNow;

                var entity = new CarerOnboardingEntity
                {
                    CarerId = data.CarerId,
                    CreatedAt = EnsureUtc(data.CreatedAt),
                    UpdatedAt = EnsureUtc(data.UpdatedAt),
                    CreatedByUserId = data.CreatedByUserId,
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    Email = data.Email,
                    PhoneNumber = data.PhoneNumber,
                    Address = data.Address,
                    StudentId = data.StudentId,
                    StudentName = data.StudentName,
                    Relationship = data.Relationship,
                    StudentDateOfBirth = EnsureUtc(data.StudentDateOfBirth),
                    EHCPStatus = data.EHCPStatus,
                    HasParentalResponsibility = data.HasParentalResponsibility,
                    NoRestrictiveOrders = data.NoRestrictiveOrders,
                    PreferredContactMethod = data.PreferredContactMethod,
                    MedicalAndAccessibilityInfo = data.MedicalAndAccessibilityInfo,
                    EmergencyContactName = data.EmergencyContactName,
                    EmergencyContactPhone = data.EmergencyContactPhone,
                    EmergencyContactRelationship = data.EmergencyContactRelationship,
                    PrivacyNoticeAcknowledged = data.ConsentsProvided.PrivacyNoticeAcknowledged,
                    PrivacyNoticeAcknowledgedAt = EnsureUtc(data.ConsentsProvided.PrivacyNoticeAcknowledgedAt),
                    DailyUpdatesConsent = data.ConsentsProvided.DailyUpdatesConsent,
                    DailyUpdatesConsentAt = EnsureUtc(data.ConsentsProvided.DailyUpdatesConsentAt),
                    DailyUpdatesWithdrawn = data.ConsentsProvided.DailyUpdatesWithdrawn,
                    PhotosVideosConsent = data.ConsentsProvided.PhotosVideosConsent,
                    PhotosVideosConsentAt = EnsureUtc(data.ConsentsProvided.PhotosVideosConsentAt),
                    PhotosVideosWithdrawn = data.ConsentsProvided.PhotosVideosWithdrawn,
                    ThirdPartySharingConsent = data.ConsentsProvided.ThirdPartySharingConsent,
                    ThirdPartySharingConsentAt = EnsureUtc(data.ConsentsProvided.ThirdPartySharingConsentAt),
                    ThirdPartySharingWithdrawn = data.ConsentsProvided.ThirdPartySharingWithdrawn,
                    LegitimateInterestConsent = data.ConsentsProvided.LegitimateInterestConsent,
                    LegitimateInterestConsentAt = EnsureUtc(data.ConsentsProvided.LegitimateInterestConsentAt),
                    TermsAccepted = data.ConsentsProvided.TermsAccepted,
                    TermsAcceptedAt = EnsureUtc(data.ConsentsProvided.TermsAcceptedAt),
                    ConfirmAccuracyAndTruth = data.ConfirmAccuracyAndTruth,
                    Status = data.Status.ToString(),
                    ApprovalNotes = data.ApprovalNotes,
                    ApprovedAt = EnsureUtc(data.ApprovedAt),
                    ApprovedByUserId = data.ApprovedByUserId,
                    DataRetentionExpiryDate = EnsureUtc(data.DataRetentionExpiryDate),
                    DataRetentionCompleted = data.DataRetentionCompleted
                };

                _dbContext.CarerOnboardings.Add(entity);
                await _dbContext.SaveChangesAsync();

                _dbContext.CarerConsentAuditEntries.Add(new CarerConsentAuditEntryEntity
                {
                    CarerOnboardingId = entity.Id,
                    Timestamp = now,
                    ConsentType = ConsentType.LegitimateInterest.ToString(),
                    Granted = data.ConsentsProvided.LegitimateInterestConsent,
                    Reason = "Initial onboarding"
                });

                await _dbContext.SaveChangesAsync();

                await LogAuditAsync(
                    eventType: "Business",
                    action: "CarerOnboardingSubmitted",
                    pagePath: "/carer-onboarding",
                    actorRole: "Anonymous",
                    actorUsername: "anonymous",
                    entityType: "CarerOnboarding",
                    entityId: data.CarerId,
                    success: true,
                    details: $"Carer onboarding submitted for {data.FirstName} {data.LastName}".Trim());

                return new CarerOnboardingResult
                {
                    Success = true,
                    CarerId = data.CarerId,
                    Message = "Onboarding submitted successfully",
                    Status = OnboardingStatus.Pending
                };
            }
            catch (Exception ex)
            {
                await LogAuditAsync(
                    eventType: "Business",
                    action: "CarerOnboardingSubmissionFailed",
                    pagePath: "/carer-onboarding",
                    actorRole: "Anonymous",
                    actorUsername: "anonymous",
                    entityType: "CarerOnboarding",
                    success: false,
                    details: ex.Message);

                return new CarerOnboardingResult
                {
                    Success = false,
                    Message = $"Failed to save onboarding: {ex.Message}",
                    Status = OnboardingStatus.Pending,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<CarerOnboardingData?> GetCarerByIdAsync(string carerId)
        {
            var entity = await _dbContext.CarerOnboardings
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CarerId == carerId);

            return entity is null ? null : MapCarer(entity);
        }

        public async Task<IEnumerable<CarerOnboardingData>> GetAllOnboardingsAsync()
        {
            var entities = await _dbContext.CarerOnboardings
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return entities.Select(MapCarer);
        }

        public async Task<IEnumerable<CarerOnboardingData>> GetPendingOnboardingsAsync()
        {
            var entities = await _dbContext.CarerOnboardings
                .AsNoTracking()
                .Where(c => c.Status == "Pending")
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return entities.Select(MapCarer);
        }

        public async Task<bool> ApproveOnboardingAsync(string carerId, string adminNotes)
        {
            var carer = await _dbContext.CarerOnboardings.FirstOrDefaultAsync(c => c.CarerId == carerId);
            if (carer == null)
            {
                return false;
            }

            carer.Status = OnboardingStatus.Approved.ToString();
            carer.ApprovalNotes = adminNotes;
            carer.ApprovedAt = DateTime.UtcNow;
            carer.ApprovedByUserId = "Admin";

            _dbContext.CarerConsentAuditEntries.Add(new CarerConsentAuditEntryEntity
            {
                CarerOnboardingId = carer.Id,
                Timestamp = DateTime.UtcNow,
                ConsentType = ConsentType.LegitimateInterest.ToString(),
                Granted = true,
                Reason = adminNotes
            });

            await _dbContext.SaveChangesAsync();

            await LogAuditAsync(
                eventType: "Business",
                action: "CarerOnboardingApproved",
                pagePath: "/admin-review",
                actorRole: "CentreAdmin",
                actorUsername: "admin",
                entityType: "CarerOnboarding",
                entityId: carerId,
                success: true,
                details: adminNotes);

            return true;
        }

        public async Task<bool> UpdateOnboardingAsync(string carerId, CarerAdminUpdateModel model, string changedBy = "Admin")
        {
            var carer = await _dbContext.CarerOnboardings.FirstOrDefaultAsync(c => c.CarerId == carerId);
            if (carer == null)
            {
                return false;
            }

            carer.FirstName = NormalizeNullable(model.FirstName);
            carer.LastName = NormalizeNullable(model.LastName);
            carer.Email = NormalizeNullable(model.Email);
            carer.PhoneNumber = NormalizeNullable(model.PhoneNumber);
            carer.Address = NormalizeNullable(model.Address);
            carer.StudentId = NormalizeNullable(model.StudentId);
            carer.StudentName = NormalizeNullable(model.StudentName);
            carer.Relationship = NormalizeNullable(model.Relationship);
            carer.EHCPStatus = NormalizeNullable(model.EHCPStatus);
            carer.PreferredContactMethod = NormalizeNullable(model.PreferredContactMethod);
            carer.EmergencyContactName = NormalizeNullable(model.EmergencyContactName);
            carer.EmergencyContactPhone = NormalizeNullable(model.EmergencyContactPhone);
            carer.Status = model.Status.ToString();
            carer.ApprovalNotes = NormalizeNullable(model.ApprovalNotes);
            carer.UpdatedAt = DateTime.UtcNow;

            _dbContext.CarerConsentAuditEntries.Add(new CarerConsentAuditEntryEntity
            {
                CarerOnboardingId = carer.Id,
                Timestamp = DateTime.UtcNow,
                ConsentType = ConsentType.LegitimateInterest.ToString(),
                Granted = true,
                Reason = $"Record updated by {changedBy}"
            });

            await _dbContext.SaveChangesAsync();
            await LogAuditAsync(
                eventType: "Business",
                action: "CarerOnboardingUpdated",
                pagePath: "/admin-review",
                actorRole: "CentreAdmin",
                actorUsername: changedBy,
                entityType: "CarerOnboarding",
                entityId: carerId,
                success: true,
                details: "Carer onboarding record updated by centre administrator.");

            return true;
        }

        public async Task<bool> UpdateConsentAsync(string carerId, ConsentUpdate consent)
        {
            var carer = await _dbContext.CarerOnboardings.FirstOrDefaultAsync(c => c.CarerId == carerId);
            if (carer == null)
            {
                return false;
            }

            // Update consent status
            switch (consent.ConsentType)
            {
                case ConsentType.DailyUpdates:
                    carer.DailyUpdatesConsent = consent.IsGranting;
                    carer.DailyUpdatesConsentAt = consent.UpdatedAt;
                    carer.DailyUpdatesWithdrawn = !consent.IsGranting;
                    break;
                case ConsentType.PhotosVideos:
                    carer.PhotosVideosConsent = consent.IsGranting;
                    carer.PhotosVideosConsentAt = consent.UpdatedAt;
                    carer.PhotosVideosWithdrawn = !consent.IsGranting;
                    break;
                case ConsentType.ThirdPartySharing:
                    carer.ThirdPartySharingConsent = consent.IsGranting;
                    carer.ThirdPartySharingConsentAt = consent.UpdatedAt;
                    carer.ThirdPartySharingWithdrawn = !consent.IsGranting;
                    break;
                case ConsentType.LegitimateInterest:
                    carer.LegitimateInterestConsent = consent.IsGranting;
                    carer.LegitimateInterestConsentAt = consent.UpdatedAt;
                    break;
            }

            carer.UpdatedAt = DateTime.UtcNow;

            _dbContext.CarerConsentAuditEntries.Add(new CarerConsentAuditEntryEntity
            {
                CarerOnboardingId = carer.Id,
                Timestamp = consent.UpdatedAt,
                ConsentType = consent.ConsentType.ToString(),
                Granted = consent.IsGranting,
                Reason = consent.Reason
            });

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCarerDataAsync(string carerId, string reason)
        {
            var carer = await _dbContext.CarerOnboardings.FirstOrDefaultAsync(c => c.CarerId == carerId);
            if (carer == null)
            {
                return false;
            }

            _dbContext.CarerOnboardings.Remove(carer);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<ConsentAuditLog> GetConsentHistoryAsync(string carerId)
        {
            var carer = await _dbContext.CarerOnboardings
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CarerId == carerId);

            if (carer == null)
            {
                return new ConsentAuditLog { CarerId = carerId };
            }

            var entries = await _dbContext.CarerConsentAuditEntries
                .AsNoTracking()
                .Where(a => a.CarerOnboardingId == carer.Id)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return new ConsentAuditLog
            {
                CarerId = carerId,
                Entries = entries.Select(e => new ConsentAuditEntry
                {
                    Timestamp = e.Timestamp,
                    ConsentType = ParseConsentType(e.ConsentType),
                    Granted = e.Granted,
                    IpAddress = e.IpAddress,
                    UserAgent = e.UserAgent,
                    Reason = e.Reason
                }).ToList()
            };
        }

        private async Task<string> GenerateNextCarerIdAsync()
        {
            var ids = await _dbContext.CarerOnboardings
                .AsNoTracking()
                .Select(c => c.CarerId)
                .ToListAsync();

            var max = 0;
            foreach (var id in ids)
            {
                if (id.StartsWith("CAR", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(id[3..], out var parsed)
                    && parsed > max)
                {
                    max = parsed;
                }
            }

            return $"CAR{max + 1:D6}";
        }

        private static CarerOnboardingData MapCarer(CarerOnboardingEntity entity)
        {
            return new CarerOnboardingData
            {
                CarerId = entity.CarerId,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedByUserId = entity.CreatedByUserId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
                StudentId = entity.StudentId,
                StudentName = entity.StudentName,
                Relationship = entity.Relationship,
                StudentDateOfBirth = entity.StudentDateOfBirth,
                EHCPStatus = entity.EHCPStatus,
                HasParentalResponsibility = entity.HasParentalResponsibility,
                NoRestrictiveOrders = entity.NoRestrictiveOrders,
                PreferredContactMethod = entity.PreferredContactMethod,
                MedicalAndAccessibilityInfo = entity.MedicalAndAccessibilityInfo,
                EmergencyContactName = entity.EmergencyContactName,
                EmergencyContactPhone = entity.EmergencyContactPhone,
                EmergencyContactRelationship = entity.EmergencyContactRelationship,
                ConsentsProvided = new ConsentStatus
                {
                    PrivacyNoticeAcknowledged = entity.PrivacyNoticeAcknowledged,
                    PrivacyNoticeAcknowledgedAt = entity.PrivacyNoticeAcknowledgedAt,
                    DailyUpdatesConsent = entity.DailyUpdatesConsent,
                    DailyUpdatesConsentAt = entity.DailyUpdatesConsentAt,
                    DailyUpdatesWithdrawn = entity.DailyUpdatesWithdrawn,
                    PhotosVideosConsent = entity.PhotosVideosConsent,
                    PhotosVideosConsentAt = entity.PhotosVideosConsentAt,
                    PhotosVideosWithdrawn = entity.PhotosVideosWithdrawn,
                    ThirdPartySharingConsent = entity.ThirdPartySharingConsent,
                    ThirdPartySharingConsentAt = entity.ThirdPartySharingConsentAt,
                    ThirdPartySharingWithdrawn = entity.ThirdPartySharingWithdrawn,
                    LegitimateInterestConsent = entity.LegitimateInterestConsent,
                    LegitimateInterestConsentAt = entity.LegitimateInterestConsentAt,
                    TermsAccepted = entity.TermsAccepted,
                    TermsAcceptedAt = entity.TermsAcceptedAt
                },
                ConfirmAccuracyAndTruth = entity.ConfirmAccuracyAndTruth,
                Status = ParseStatus(entity.Status),
                ApprovalNotes = entity.ApprovalNotes,
                ApprovedAt = entity.ApprovedAt,
                ApprovedByUserId = entity.ApprovedByUserId,
                DataRetentionExpiryDate = entity.DataRetentionExpiryDate,
                DataRetentionCompleted = entity.DataRetentionCompleted
            };
        }

        private static OnboardingStatus ParseStatus(string? status)
        {
            return Enum.TryParse<OnboardingStatus>(status, true, out var parsed)
                ? parsed
                : OnboardingStatus.Pending;
        }

        private static ConsentType ParseConsentType(string? consentType)
        {
            return Enum.TryParse<ConsentType>(consentType, true, out var parsed)
                ? parsed
                : ConsentType.LegitimateInterest;
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

        private static string? NormalizeNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
