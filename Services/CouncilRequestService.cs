using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using StudyMgt.Data.Entities;

namespace StudyMgt.Services;

public class CouncilRequestService
{
    private const string ActionName = "CouncilRequestSubmitted";
    private const string ReviewedActionName = "CouncilRequestReviewed";
    private const string ApprovedActionName = "CouncilRequestApproved";
    private const string DeclinedActionName = "CouncilRequestDeclined";
    private const string PagePath = "/council-representative-portal";
    private const string AdminPagePath = "/centre-administrators";
    private static readonly Regex StudentIdReferencePattern = new("^[A-Za-z0-9\\-/]+$", RegexOptions.Compiled);
    private readonly AppAuditLogService _auditLog;

    public CouncilRequestService(AppAuditLogService auditLog)
    {
        _auditLog = auditLog;
    }

    public async Task SubmitRequestAsync(CouncilRequestSubmission submission)
    {
        submission.StudentIdentifier = submission.StudentIdentifier?.Trim() ?? string.Empty;

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(submission);
        if (!Validator.TryValidateObject(submission, validationContext, validationResults, validateAllProperties: true))
        {
            throw new ValidationException(validationResults[0].ErrorMessage ?? "Invalid council request submission.");
        }

        if (!StudentIdReferencePattern.IsMatch(submission.StudentIdentifier))
        {
            throw new ValidationException("Student ID/reference number can only include letters, numbers, '-' or '/'.");
        }

        var payload = new CouncilRequestPayload
        {
            RepresentativeName = submission.RepresentativeName,
            CouncilName = submission.CouncilName,
            Email = submission.Email,
            StudentIdentifier = submission.StudentIdentifier,
            YoungPersonName = submission.YoungPersonName,
            DateOfBirth = submission.DateOfBirth,
            Gender = submission.Gender,
            FirstLanguage = submission.FirstLanguage,
            Address = submission.Address,
            ParentCarer = submission.ParentCarer,
            HomeTel = submission.HomeTel,
            Mobile = submission.Mobile,
            LastSchoolName = submission.LastSchoolName,
            CurrentYearGroup = submission.CurrentYearGroup,
            SettingType = submission.SettingType,
            SendInformation = submission.SendInformation,
            AttainmentReading = submission.AttainmentReading,
            AttainmentWriting = submission.AttainmentWriting,
            AttainmentMaths = submission.AttainmentMaths,
            AttainmentScience = submission.AttainmentScience,
            TotalHoursRequired = submission.TotalHoursRequired,
            Frequency = submission.Frequency,
            RequestedDays = submission.RequestedDays,
            TuitionRequirements = submission.TuitionRequirements,
            LessonLocation = submission.LessonLocation,
            InvoiceRecipient = submission.InvoiceRecipient,
            RequestType = submission.RequestType,
            PreferredContactMethod = submission.PreferredContactMethod,
            Details = submission.Details,
            AttachmentNames = submission.AttachmentNames,
            AttachmentCount = submission.AttachmentNames.Count
        };

        await _auditLog.LogAsync(
            eventType: "Council",
            action: ActionName,
            pagePath: PagePath,
            actorRole: "CouncilRepresentative",
            actorUsername: submission.Email,
            entityType: "CouncilRequest",
            entityId: submission.StudentIdentifier,
            success: true,
            details: JsonSerializer.Serialize(payload));
    }

    public async Task<List<CouncilRequestListItem>> GetRecentRequestsAsync(int take = 100)
    {
        var logs = await _auditLog.GetLogsAsync(
            action: ActionName,
            pagePath: PagePath,
            success: true,
            take: take);

        var requestIdStrings = logs
            .Select(x => x.Id.ToString())
            .ToHashSet(StringComparer.Ordinal);

        var approvalLogs = await _auditLog.GetLogsAsync(
            action: ApprovedActionName,
            pagePath: AdminPagePath,
            success: true,
            take: Math.Max(500, take * 5));

        var declineLogs = await _auditLog.GetLogsAsync(
            action: DeclinedActionName,
            pagePath: AdminPagePath,
            success: true,
            take: Math.Max(500, take * 5));

        var reviewLogs = await _auditLog.GetLogsAsync(
            action: ReviewedActionName,
            pagePath: AdminPagePath,
            success: true,
            take: Math.Max(500, take * 5));

        var approvalMap = new Dictionary<long, (DateTime ApprovedAtUtc, string ApprovedBy, string? Notes)>(capacity: logs.Count);
        foreach (var approvalLog in approvalLogs)
        {
            if (!string.Equals(approvalLog.EntityType, "ApplicationAuditLog", StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(approvalLog.EntityId) ||
                !requestIdStrings.Contains(approvalLog.EntityId) ||
                !long.TryParse(approvalLog.EntityId, out var requestLogId) ||
                approvalMap.ContainsKey(requestLogId))
            {
                continue;
            }

            var payload = TryDeserializeApproval(approvalLog.Details);
            approvalMap[requestLogId] = (
                ApprovedAtUtc: payload?.ReviewedAtUtc ?? approvalLog.OccurredAtUtc,
                ApprovedBy: payload?.ReviewedBy ?? approvalLog.ActorUsername ?? "centre-admin",
                Notes: payload?.Notes);
        }

        var reviewedRequestIds = reviewLogs
            .Where(x =>
                string.Equals(x.EntityType, "ApplicationAuditLog", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(x.EntityId) &&
                long.TryParse(x.EntityId, out _))
            .Select(x => long.Parse(x.EntityId!, System.Globalization.CultureInfo.InvariantCulture))
            .ToHashSet();

        var declinedMap = new Dictionary<long, (DateTime DeclinedAtUtc, string DeclinedBy, string? Notes)>(capacity: logs.Count);
        foreach (var declineLog in declineLogs)
        {
            if (!string.Equals(declineLog.EntityType, "ApplicationAuditLog", StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(declineLog.EntityId) ||
                !requestIdStrings.Contains(declineLog.EntityId) ||
                !long.TryParse(declineLog.EntityId, out var requestLogId) ||
                declinedMap.ContainsKey(requestLogId))
            {
                continue;
            }

            var payload = TryDeserializeApproval(declineLog.Details);
            declinedMap[requestLogId] = (
                DeclinedAtUtc: payload?.ReviewedAtUtc ?? declineLog.OccurredAtUtc,
                DeclinedBy: payload?.ReviewedBy ?? declineLog.ActorUsername ?? "centre-admin",
                Notes: payload?.Notes);
        }

        var results = new List<CouncilRequestListItem>(logs.Count);

        foreach (var log in logs)
        {
            var payload = TryDeserialize(log.Details);
            approvalMap.TryGetValue(log.Id, out var approval);
            declinedMap.TryGetValue(log.Id, out var decline);
            CouncilRequestApprovalPayload? approvalPayload = approval != default
                ? new CouncilRequestApprovalPayload
                {
                    ReviewedAtUtc = approval.ApprovedAtUtc,
                    ReviewedBy = approval.ApprovedBy,
                    Notes = approval.Notes
                }
                : null;
            CouncilRequestApprovalPayload? declinePayload = decline != default
                ? new CouncilRequestApprovalPayload
                {
                    ReviewedAtUtc = decline.DeclinedAtUtc,
                    ReviewedBy = decline.DeclinedBy,
                    Notes = decline.Notes
                }
                : null;

            results.Add(BuildCouncilRequestListItem(
                log,
                payload,
                approvalPayload,
                declinePayload,
                reviewedRequestIds.Contains(log.Id)));
        }

        return results;
    }

    public async Task<CouncilRequestListItem?> GetRequestByIdAsync(long requestLogId)
    {
        if (requestLogId <= 0)
        {
            return null;
        }

        var requestLog = await _auditLog.GetLogByIdAsync(requestLogId);
        if (requestLog is null ||
            !string.Equals(requestLog.Action, ActionName, StringComparison.Ordinal) ||
            !string.Equals(requestLog.PagePath, PagePath, StringComparison.Ordinal))
        {
            return null;
        }

        var approvalLogs = await _auditLog.GetLogsAsync(
            action: ApprovedActionName,
            pagePath: AdminPagePath,
            success: true,
            take: 1000);

        var declineLogs = await _auditLog.GetLogsAsync(
            action: DeclinedActionName,
            pagePath: AdminPagePath,
            success: true,
            take: 1000);

        var reviewLogs = await _auditLog.GetLogsAsync(
            action: ReviewedActionName,
            pagePath: AdminPagePath,
            success: true,
            take: 1000);

        var approvalLog = approvalLogs.FirstOrDefault(x =>
            string.Equals(x.EntityType, "ApplicationAuditLog", StringComparison.Ordinal) &&
            string.Equals(x.EntityId, requestLogId.ToString(), StringComparison.Ordinal));

        var reviewLog = reviewLogs.FirstOrDefault(x =>
            string.Equals(x.EntityType, "ApplicationAuditLog", StringComparison.Ordinal) &&
            string.Equals(x.EntityId, requestLogId.ToString(), StringComparison.Ordinal));

        var declineLog = declineLogs.FirstOrDefault(x =>
            string.Equals(x.EntityType, "ApplicationAuditLog", StringComparison.Ordinal) &&
            string.Equals(x.EntityId, requestLogId.ToString(), StringComparison.Ordinal));

        var payload = TryDeserialize(requestLog.Details);
        CouncilRequestApprovalPayload? approvalPayload = null;
        if (approvalLog is not null)
        {
            approvalPayload = TryDeserializeApproval(approvalLog.Details) ?? new CouncilRequestApprovalPayload
            {
                ReviewedBy = approvalLog.ActorUsername ?? "centre-admin",
                ReviewedAtUtc = approvalLog.OccurredAtUtc
            };
        }

        CouncilRequestApprovalPayload? declinePayload = null;
        if (declineLog is not null)
        {
            declinePayload = TryDeserializeApproval(declineLog.Details) ?? new CouncilRequestApprovalPayload
            {
                ReviewedBy = declineLog.ActorUsername ?? "centre-admin",
                ReviewedAtUtc = declineLog.OccurredAtUtc
            };
        }

        return BuildCouncilRequestListItem(
            requestLog,
            payload,
            approvalPayload,
            declinePayload,
            reviewLog is not null);
    }

    public async Task<CouncilRequestListItem?> GetRequestByStudentIdentifierAsync(string studentIdentifier)
    {
        var normalizedIdentifier = studentIdentifier?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedIdentifier))
        {
            return null;
        }

        var logs = await _auditLog.GetLogsAsync(
            action: ActionName,
            pagePath: PagePath,
            success: true,
            take: 1000);

        var matchingLog = logs
            .Select(log => new { Log = log, Payload = TryDeserialize(log.Details) })
            .Where(x =>
                string.Equals(x.Payload?.StudentIdentifier?.Trim(), normalizedIdentifier, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Log.EntityId?.Trim(), normalizedIdentifier, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.Log.OccurredAtUtc)
            .FirstOrDefault();

        if (matchingLog is null)
        {
            return null;
        }

        return await GetRequestByIdAsync(matchingLog.Log.Id);
    }

    public async Task<(bool Success, string Message)> ApproveRequestAsync(long requestLogId, string approvedBy, string? notes)
    {
        if (requestLogId <= 0)
        {
            return (false, "Invalid council request identifier.");
        }

        var requestLog = await _auditLog.GetLogByIdAsync(requestLogId);
        if (requestLog is null || !string.Equals(requestLog.Action, ActionName, StringComparison.Ordinal))
        {
            return (false, "Council request not found.");
        }

        var approvalPayload = new CouncilRequestApprovalPayload
        {
            RequestLogId = requestLogId,
            Status = "Approved",
            ReviewedBy = string.IsNullOrWhiteSpace(approvedBy) ? "centre-admin" : approvedBy.Trim(),
            ReviewedAtUtc = DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        await _auditLog.LogAsync(
            eventType: "Council",
            action: ApprovedActionName,
            pagePath: AdminPagePath,
            actorRole: "CentreAdmin",
            actorUsername: approvalPayload.ReviewedBy,
            entityType: "ApplicationAuditLog",
            entityId: requestLogId.ToString(),
            success: true,
            details: JsonSerializer.Serialize(approvalPayload));

        return (true, "Council request accepted.");
    }

    public async Task<(bool Success, string Message)> ReviewRequestAsync(long requestLogId, string reviewedBy, string? notes)
    {
        if (requestLogId <= 0)
        {
            return (false, "Invalid council request identifier.");
        }

        var requestLog = await _auditLog.GetLogByIdAsync(requestLogId);
        if (requestLog is null || !string.Equals(requestLog.Action, ActionName, StringComparison.Ordinal))
        {
            return (false, "Council request not found.");
        }

        var reviewPayload = new CouncilRequestReviewPayload
        {
            RequestLogId = requestLogId,
            Status = "Reviewed",
            ReviewedBy = string.IsNullOrWhiteSpace(reviewedBy) ? "centre-admin" : reviewedBy.Trim(),
            ReviewedAtUtc = DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        await _auditLog.LogAsync(
            eventType: "Council",
            action: ReviewedActionName,
            pagePath: AdminPagePath,
            actorRole: "CentreAdmin",
            actorUsername: reviewPayload.ReviewedBy,
            entityType: "ApplicationAuditLog",
            entityId: requestLogId.ToString(),
            success: true,
            details: JsonSerializer.Serialize(reviewPayload));

        return (true, "Council request marked as reviewed.");
    }

    public async Task<(bool Success, string Message)> DeclineRequestAsync(long requestLogId, string declinedBy, string? notes)
    {
        if (requestLogId <= 0)
        {
            return (false, "Invalid council request identifier.");
        }

        var requestLog = await _auditLog.GetLogByIdAsync(requestLogId);
        if (requestLog is null || !string.Equals(requestLog.Action, ActionName, StringComparison.Ordinal))
        {
            return (false, "Council request not found.");
        }

        var reviewPayload = new CouncilRequestApprovalPayload
        {
            RequestLogId = requestLogId,
            Status = "Declined",
            ReviewedBy = string.IsNullOrWhiteSpace(declinedBy) ? "centre-admin" : declinedBy.Trim(),
            ReviewedAtUtc = DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        await _auditLog.LogAsync(
            eventType: "Council",
            action: DeclinedActionName,
            pagePath: AdminPagePath,
            actorRole: "CentreAdmin",
            actorUsername: reviewPayload.ReviewedBy,
            entityType: "ApplicationAuditLog",
            entityId: requestLogId.ToString(),
            success: true,
            details: JsonSerializer.Serialize(reviewPayload));

        return (true, "Council request declined.");
    }

    private static CouncilRequestPayload? TryDeserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CouncilRequestPayload>(json);
        }
        catch
        {
            return null;
        }
    }

    private static CouncilRequestApprovalPayload? TryDeserializeApproval(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CouncilRequestApprovalPayload>(json);
        }
        catch
        {
            return null;
        }
    }

    private static CouncilRequestListItem BuildCouncilRequestListItem(
        ApplicationAuditLogEntity requestLog,
        CouncilRequestPayload? payload,
        CouncilRequestApprovalPayload? approvalPayload,
        CouncilRequestApprovalPayload? declinePayload,
        bool isReviewed)
    {
        var isApproved = approvalPayload is not null;
        var isDeclined = declinePayload is not null;

        return new CouncilRequestListItem
        {
            RequestLogId = requestLog.Id,
            ReceivedAtUtc = requestLog.OccurredAtUtc,
            RepresentativeName = payload?.RepresentativeName ?? "-",
            CouncilName = payload?.CouncilName ?? "-",
            Email = payload?.Email ?? requestLog.ActorUsername ?? "-",
            StudentIdentifier = payload?.StudentIdentifier ?? requestLog.EntityId ?? "-",
            YoungPersonName = payload?.YoungPersonName ?? "-",
            DateOfBirth = payload?.DateOfBirth,
            Gender = payload?.Gender,
            FirstLanguage = payload?.FirstLanguage,
            Address = payload?.Address,
            ParentCarer = payload?.ParentCarer,
            HomeTel = payload?.HomeTel,
            Mobile = payload?.Mobile,
            LastSchoolName = payload?.LastSchoolName,
            CurrentYearGroup = payload?.CurrentYearGroup,
            SettingType = payload?.SettingType,
            SendInformation = payload?.SendInformation,
            AttainmentReading = payload?.AttainmentReading,
            AttainmentWriting = payload?.AttainmentWriting,
            AttainmentMaths = payload?.AttainmentMaths,
            AttainmentScience = payload?.AttainmentScience,
            TotalHoursRequired = payload?.TotalHoursRequired,
            Frequency = payload?.Frequency,
            RequestedDays = payload?.RequestedDays,
            TuitionRequirements = payload?.TuitionRequirements,
            LessonLocation = payload?.LessonLocation,
            InvoiceRecipient = payload?.InvoiceRecipient,
            RequestType = payload?.RequestType ?? "-",
            PreferredContactMethod = payload?.PreferredContactMethod ?? "-",
            Details = payload?.Details ?? "-",
            AttachmentCount = payload?.AttachmentCount ?? 0,
            AttachmentNames = payload?.AttachmentNames ?? Array.Empty<string>(),
            ApprovalStatus = isApproved ? "Approved" : isDeclined ? "Declined" : isReviewed ? "Reviewed" : "Pending",
            ApprovedAtUtc = isApproved ? approvalPayload?.ReviewedAtUtc : isDeclined ? declinePayload?.ReviewedAtUtc : null,
            ApprovedBy = isApproved ? approvalPayload?.ReviewedBy : isDeclined ? declinePayload?.ReviewedBy : null,
            ApprovalNotes = isApproved ? approvalPayload?.Notes : isDeclined ? declinePayload?.Notes : null
        };
    }

    private sealed class CouncilRequestPayload
    {
        public string RepresentativeName { get; set; } = string.Empty;
        public string CouncilName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string StudentIdentifier { get; set; } = string.Empty;
        public string YoungPersonName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? FirstLanguage { get; set; }
        public string? Address { get; set; }
        public string? ParentCarer { get; set; }
        public string? HomeTel { get; set; }
        public string? Mobile { get; set; }
        public string? LastSchoolName { get; set; }
        public string? CurrentYearGroup { get; set; }
        public string? SettingType { get; set; }
        public string? SendInformation { get; set; }
        public string? AttainmentReading { get; set; }
        public string? AttainmentWriting { get; set; }
        public string? AttainmentMaths { get; set; }
        public string? AttainmentScience { get; set; }
        public string? TotalHoursRequired { get; set; }
        public string? Frequency { get; set; }
        public string? RequestedDays { get; set; }
        public string? TuitionRequirements { get; set; }
        public string? LessonLocation { get; set; }
        public string? InvoiceRecipient { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string PreferredContactMethod { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int AttachmentCount { get; set; }
        public IReadOnlyList<string> AttachmentNames { get; set; } = Array.Empty<string>();
    }

    private sealed class CouncilRequestApprovalPayload
    {
        public long RequestLogId { get; set; }
        public string Status { get; set; } = "Approved";
        public string ReviewedBy { get; set; } = "centre-admin";
        public DateTime ReviewedAtUtc { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }

    private sealed class CouncilRequestReviewPayload
    {
        public long RequestLogId { get; set; }
        public string Status { get; set; } = "Reviewed";
        public string ReviewedBy { get; set; } = "centre-admin";
        public DateTime ReviewedAtUtc { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }

    public sealed class CouncilRequestSubmission : IValidatableObject
    {
        [Required]
        public string RepresentativeName { get; set; } = string.Empty;

        [Required]
        public string CouncilName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Student ID / Reference Number")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Student ID/reference number must be between 3 and 50 characters.")]
        public string StudentIdentifier { get; set; } = string.Empty;

        [Required]
        public string YoungPersonName { get; set; } = string.Empty;

        [Required]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string FirstLanguage { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string ParentCarer { get; set; } = string.Empty;

        [Required]
        public string HomeTel { get; set; } = string.Empty;

        [Required]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        public string LastSchoolName { get; set; } = string.Empty;

        [Required]
        public string CurrentYearGroup { get; set; } = string.Empty;

        [Required]
        public string SettingType { get; set; } = string.Empty;

        [Required]
        public string SendInformation { get; set; } = string.Empty;

        public bool AttainmentUnknown { get; set; }

        public string AttainmentReading { get; set; } = string.Empty;

        public string AttainmentWriting { get; set; } = string.Empty;

        public string AttainmentMaths { get; set; } = string.Empty;

        public string AttainmentScience { get; set; } = string.Empty;

        [Required]
        public string TotalHoursRequired { get; set; } = string.Empty;

        [Required]
        public string Frequency { get; set; } = string.Empty;

        [Required]
        public string RequestedDays { get; set; } = string.Empty;

        [Required]
        public string TuitionRequirements { get; set; } = string.Empty;

        [Required]
        public string LessonLocation { get; set; } = string.Empty;

        [Required]
        public string InvoiceRecipient { get; set; } = string.Empty;

        [Required]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        public string PreferredContactMethod { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        public string Details { get; set; } = string.Empty;

        public IReadOnlyList<string> AttachmentNames { get; set; } = Array.Empty<string>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AttainmentUnknown)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(AttainmentReading))
            {
                yield return new ValidationResult("Attainment reading is required unless Unknown is selected.", new[] { nameof(AttainmentReading) });
            }

            if (string.IsNullOrWhiteSpace(AttainmentWriting))
            {
                yield return new ValidationResult("Attainment writing is required unless Unknown is selected.", new[] { nameof(AttainmentWriting) });
            }

            if (string.IsNullOrWhiteSpace(AttainmentMaths))
            {
                yield return new ValidationResult("Attainment maths is required unless Unknown is selected.", new[] { nameof(AttainmentMaths) });
            }

            if (string.IsNullOrWhiteSpace(AttainmentScience))
            {
                yield return new ValidationResult("Attainment science is required unless Unknown is selected.", new[] { nameof(AttainmentScience) });
            }
        }
    }

    public sealed class CouncilRequestListItem
    {
        public long RequestLogId { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public string RepresentativeName { get; set; } = "-";
        public string CouncilName { get; set; } = "-";
        public string Email { get; set; } = "-";
        public string StudentIdentifier { get; set; } = "-";
        public string YoungPersonName { get; set; } = "-";
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? FirstLanguage { get; set; }
        public string? Address { get; set; }
        public string? ParentCarer { get; set; }
        public string? HomeTel { get; set; }
        public string? Mobile { get; set; }
        public string? LastSchoolName { get; set; }
        public string? CurrentYearGroup { get; set; }
        public string? SettingType { get; set; }
        public string? SendInformation { get; set; }
        public string? AttainmentReading { get; set; }
        public string? AttainmentWriting { get; set; }
        public string? AttainmentMaths { get; set; }
        public string? AttainmentScience { get; set; }
        public string? TotalHoursRequired { get; set; }
        public string? Frequency { get; set; }
        public string? RequestedDays { get; set; }
        public string? TuitionRequirements { get; set; }
        public string? LessonLocation { get; set; }
        public string? InvoiceRecipient { get; set; }
        public string RequestType { get; set; } = "-";
        public string PreferredContactMethod { get; set; } = "-";
        public string Details { get; set; } = "-";
        public int AttachmentCount { get; set; }
        public IReadOnlyList<string> AttachmentNames { get; set; } = Array.Empty<string>();
        public string ApprovalStatus { get; set; } = "Pending Review";
        public DateTime? ApprovedAtUtc { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ApprovalNotes { get; set; }
    }
}