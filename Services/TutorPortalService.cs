using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudyMgt.Data;
using StudyMgt.Data.Entities;

namespace StudyMgt.Services;

public class TutorPortalSession
{
    public bool IsAuthenticated { get; private set; }
    public int? TutorOnboardingId { get; private set; }
    public string? TutorName { get; private set; }
    public string? Username { get; private set; }
    public int? CurrentSessionLogId { get; private set; }

    public void SignIn(int tutorOnboardingId, string tutorName, string username, int? sessionLogId = null)
    {
        IsAuthenticated = true;
        TutorOnboardingId = tutorOnboardingId;
        TutorName = tutorName;
        Username = username;
        CurrentSessionLogId = sessionLogId;
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        TutorOnboardingId = null;
        TutorName = null;
        Username = null;
        CurrentSessionLogId = null;
    }
}

public class TutorPortalProfileView
{
    public int TutorOnboardingId { get; set; }
    public string TutorDisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string HighestQualification { get; set; } = string.Empty;
    public string TeachingExperience { get; set; } = string.Empty;
    public string? CoursesToBeTaken { get; set; }
    public string? CourseDuration { get; set; }
    public string DBSStatus { get; set; } = string.Empty;
    public string RightToWorkStatus { get; set; } = string.Empty;
    public string SafeguardingTrainingStatus { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public string OnboardingStatus { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
}

public class TutorPortalStudentView
{
    public int StudentOnboardingId { get; set; }
    public string StudentDisplayName { get; set; } = string.Empty;
    public string? StudentIdentifier { get; set; }
    public string? AssignedTutorNames { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? EHCPStatus { get; set; }
    public string? SENIndicators { get; set; }
    public string? PreferredContactMethod { get; set; }
    public DateTime? AssignedDate { get; set; }
}

public class TutorStudentAttendanceStateView
{
    public int StudentOnboardingId { get; set; }
    public bool HasActiveLecture { get; set; }
    public DateTime? ActiveLectureStartUtc { get; set; }
    public DateTime? LastLectureEndUtc { get; set; }
    public int? LastDurationMinutes { get; set; }
}

public class TutorStudentAttendanceActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? AttendanceId { get; set; }
}

public class TutorMonthlyTimesheetView
{
    public int TutorOnboardingId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int SessionCount { get; set; }
    public int TotalMinutes { get; set; }
    public double TotalHours => Math.Round(TotalMinutes / 60d, 2);
    public DateTime GeneratedAtUtc { get; set; }
}

public class TutorMonthlyTimesheetDayView
{
    public DateTime AttendanceDateUtc { get; set; }
    public int SessionCount { get; set; }
    public int TotalMinutes { get; set; }
    public double TotalHours => Math.Round(TotalMinutes / 60d, 2);
    public DateTime UpdatedAtUtc { get; set; }
}

public class TutorMonthlyTimesheetDetailGroupData
{
    public int TutorOnboardingId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public List<TutorMonthlyTimesheetDayView> Details { get; set; } = new();
}

public class TutorPortalAuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? TutorOnboardingId { get; set; }
    public string? TutorName { get; set; }
    public string? Username { get; set; }
    public int? RemainingLoginAttempts { get; set; }
    public bool IsLockedOut { get; set; }
    public int? LockoutRemainingMinutes { get; set; }
    public int? SessionLogId { get; set; }
}

public class TutorTimeOffBookingCreateModel
{
    public string RequestType { get; set; } = "Holiday";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class TutorTimeOffBookingView
{
    public long RequestLogId { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = "Pending";
    public DateTime? ReviewedAtUtc { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewNotes { get; set; }
}

public class TutorTimeOffApprovalQueueItem
{
    public long RequestLogId { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public int TutorOnboardingId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = "Pending";
    public DateTime? ReviewedAtUtc { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewNotes { get; set; }
}

public class TutorPortalService
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private const string TutorTimeOffAction = "TutorTimeOffBooked";
    private const string TutorTimeOffReviewAction = "TutorTimeOffReviewed";
    private const string TutorPortalPagePath = "/tutor-portal";
    private const string CentreAdminPagePath = "/centre-administrators";

    private readonly StudyMgtDbContext _db;

    public TutorPortalService(StudyMgtDbContext db)
    {
        _db = db;
    }

    public async Task<TutorPortalAuthResult> RegisterAsync(int tutorOnboardingId, string email, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new TutorPortalAuthResult { Success = false, Message = "All fields are required." };
        }

        if (password.Length < 8)
        {
            return new TutorPortalAuthResult { Success = false, Message = "Password must be at least 8 characters." };
        }

        TutorOnboardingEntity? tutor;
        var normalizedEmail = email.Trim();

        if (tutorOnboardingId > 0)
        {
            tutor = await _db.TutorOnboardings
                .FirstOrDefaultAsync(t => t.Id == tutorOnboardingId && t.Email == normalizedEmail);
            if (tutor == null)
            {
                return new TutorPortalAuthResult { Success = false, Message = "Tutor record not found for the supplied ID and email." };
            }
        }
        else
        {
            var matchingTutors = await _db.TutorOnboardings
                .Where(t => t.Email == normalizedEmail)
                .Where(t => t.Status == OnboardingStatus.Approved.ToString())
                .Where(t => !_db.TutorPortalAccounts.Any(a => a.TutorOnboardingId == t.Id))
                .OrderByDescending(t => t.ApprovedDate)
                .ThenByDescending(t => t.Id)
                .Take(2)
                .ToListAsync();

            if (matchingTutors.Count == 0)
            {
                return new TutorPortalAuthResult { Success = false, Message = "Tutor record not found for the supplied email." };
            }

            if (matchingTutors.Count > 1)
            {
                return new TutorPortalAuthResult
                {
                    Success = false,
                    Message = "Multiple approved tutor records use this email. Use a unique tutor email or contact support to resolve duplicate records."
                };
            }

            tutor = matchingTutors[0];
        }

        if (!string.Equals(tutor.Status, OnboardingStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return new TutorPortalAuthResult { Success = false, Message = "Only approved tutors can create a portal login." };
        }

        if (await _db.TutorPortalAccounts.AnyAsync(a => a.Username == username))
        {
            return new TutorPortalAuthResult { Success = false, Message = "Username is already taken." };
        }

        if (await _db.TutorPortalAccounts.AnyAsync(a => a.TutorOnboardingId == tutor.Id))
        {
            return new TutorPortalAuthResult { Success = false, Message = "An account already exists for this tutor." };
        }

        CreatePasswordHash(password, out var hash, out var salt);

        _db.TutorPortalAccounts.Add(new TutorPortalAccountEntity
        {
            TutorOnboardingId = tutor.Id,
            Username = username,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return new TutorPortalAuthResult
        {
            Success = true,
            Message = "Account created successfully.",
            TutorOnboardingId = tutor.Id,
            TutorName = $"{tutor.FirstName} {tutor.LastName}".Trim(),
            Username = username
        };
    }

    public async Task<TutorPortalAuthResult> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new TutorPortalAuthResult { Success = false, Message = "Username and password are required." };
        }

        var normalizedUsername = username.Trim();

        var account = await _db.TutorPortalAccounts
            .Include(a => a.TutorOnboarding)
            .FirstOrDefaultAsync(a => a.Username == normalizedUsername && a.IsActive);

        if (account == null)
        {
            return new TutorPortalAuthResult { Success = false, Message = "Invalid username or password." };
        }

        var lockoutRemaining = await GetLockoutRemainingAsync(account.Username);
        if (lockoutRemaining.HasValue)
        {
            var remainingMinutes = Math.Max(1, (int)Math.Ceiling(lockoutRemaining.Value.TotalMinutes));
            return new TutorPortalAuthResult
            {
                Success = false,
                IsLockedOut = true,
                LockoutRemainingMinutes = remainingMinutes,
                Message = $"Your account is temporarily locked due to repeated failed login attempts. Try again in {remainingMinutes} minute(s)."
            };
        }

        if (!string.Equals(account.TutorOnboarding.Status, OnboardingStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return new TutorPortalAuthResult { Success = false, Message = "Your onboarding is not approved yet. Please contact the centre." };
        }

        if (!VerifyPasswordHash(password, account.PasswordHash, account.PasswordSalt))
        {
            var recentFailedAttempts = await GetRecentFailedAttemptsCountAsync(account.Username);
            var failedAttemptsIncludingCurrent = recentFailedAttempts + 1;
            var remainingAttempts = MaxFailedLoginAttempts - failedAttemptsIncludingCurrent;

            if (remainingAttempts <= 0)
            {
                var lockoutMinutes = Math.Max(1, (int)Math.Ceiling(LockoutDuration.TotalMinutes));
                return new TutorPortalAuthResult
                {
                    Success = false,
                    IsLockedOut = true,
                    LockoutRemainingMinutes = lockoutMinutes,
                    RemainingLoginAttempts = 0,
                    Message = $"Your account is temporarily locked due to repeated failed login attempts. Try again in {lockoutMinutes} minute(s)."
                };
            }

            return new TutorPortalAuthResult
            {
                Success = false,
                RemainingLoginAttempts = remainingAttempts,
                Message = $"Invalid username or password. {remainingAttempts} login attempt(s) remaining before temporary lockout."
            };
        }

        var loginAtUtc = DateTime.UtcNow;
        account.LastLoginAtUtc = loginAtUtc;

        var tutorDisplayName = $"{account.TutorOnboarding.FirstName} {account.TutorOnboarding.LastName}".Trim();
        await GenerateMonthlyTimesheetsIfRequiredAsync(account.TutorOnboardingId, account.Username, tutorDisplayName);

        var sessionLog = new TutorPortalSessionLogEntity
        {
            TutorPortalAccountId = account.Id,
            TutorOnboardingId = account.TutorOnboardingId,
            Username = account.Username,
            LoginAtUtc = loginAtUtc,
            IsClosed = false
        };

        _db.TutorPortalSessionLogs.Add(sessionLog);
        await _db.SaveChangesAsync();

        return new TutorPortalAuthResult
        {
            Success = true,
            Message = "Login successful.",
            TutorOnboardingId = account.TutorOnboardingId,
            TutorName = tutorDisplayName,
            Username = account.Username,
            SessionLogId = sessionLog.Id
        };
    }

    public async Task<TutorPortalAuthResult> ResetPasswordAsync(string username, string email, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
        {
            return new TutorPortalAuthResult
            {
                Success = false,
                Message = "Username, onboarding email, and new password are required."
            };
        }

        if (newPassword.Length < 8)
        {
            return new TutorPortalAuthResult
            {
                Success = false,
                Message = "New password must be at least 8 characters."
            };
        }

        var normalizedUsername = username.Trim();
        var normalizedEmail = email.Trim();

        var account = await _db.TutorPortalAccounts
            .Include(a => a.TutorOnboarding)
            .FirstOrDefaultAsync(a => a.Username == normalizedUsername && a.IsActive);

        if (account == null)
        {
            return new TutorPortalAuthResult { Success = false, Message = "Invalid reset details." };
        }

        var onboardingEmail = account.TutorOnboarding.Email?.Trim() ?? string.Empty;
        if (!string.Equals(onboardingEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            return new TutorPortalAuthResult { Success = false, Message = "Invalid reset details." };
        }

        if (!string.Equals(account.TutorOnboarding.Status, OnboardingStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return new TutorPortalAuthResult
            {
                Success = false,
                Message = "Your onboarding is not approved yet. Please contact the centre."
            };
        }

        CreatePasswordHash(newPassword, out var hash, out var salt);
        account.PasswordHash = hash;
        account.PasswordSalt = salt;
        await _db.SaveChangesAsync();

        return new TutorPortalAuthResult
        {
            Success = true,
            Message = "Password reset successful. You can now sign in with your new password.",
            TutorOnboardingId = account.TutorOnboardingId,
            TutorName = $"{account.TutorOnboarding.FirstName} {account.TutorOnboarding.LastName}".Trim(),
            Username = account.Username
        };
    }

    public async Task RecordLogoutAsync(int tutorOnboardingId, string username, int? sessionLogId)
    {
        var now = DateTime.UtcNow;

        TutorPortalSessionLogEntity? session;
        if (sessionLogId.HasValue)
        {
            session = await _db.TutorPortalSessionLogs
                .FirstOrDefaultAsync(x =>
                    x.Id == sessionLogId.Value
                    && x.TutorOnboardingId == tutorOnboardingId
                    && !x.IsClosed);
        }
        else
        {
            session = await _db.TutorPortalSessionLogs
                .Where(x =>
                    x.TutorOnboardingId == tutorOnboardingId
                    && x.Username == username
                    && !x.IsClosed)
                .OrderByDescending(x => x.LoginAtUtc)
                .FirstOrDefaultAsync();
        }

        if (session == null)
        {
            return;
        }

        session.LogoutAtUtc = now;
        session.DurationMinutes = Math.Max(0, (int)Math.Round((now - session.LoginAtUtc).TotalMinutes));
        session.IsClosed = true;

        await _db.SaveChangesAsync();

        var tutorDisplayName = await GetTutorDisplayNameAsync(tutorOnboardingId);
        await GenerateMonthlyTimesheetsIfRequiredAsync(tutorOnboardingId, username, tutorDisplayName);
    }

    private async Task<TimeSpan?> GetLockoutRemainingAsync(string username)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.Subtract(LockoutWindow);

        var failures = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(log =>
                log.Action == "TutorPortalLoginFailed"
                && log.ActorRole == "Tutor"
                && log.PagePath == "/tutor-portal"
                && log.Success == false
                && log.ActorUsername == username
                && log.OccurredAtUtc >= windowStart)
            .OrderByDescending(log => log.OccurredAtUtc)
            .Take(MaxFailedLoginAttempts)
            .ToListAsync();

        if (failures.Count < MaxFailedLoginAttempts)
        {
            return null;
        }

        var latestFailureAt = failures[0].OccurredAtUtc;
        var lockoutEndsAt = latestFailureAt.Add(LockoutDuration);

        return lockoutEndsAt > now
            ? lockoutEndsAt - now
            : null;
    }

    private async Task<int> GetRecentFailedAttemptsCountAsync(string username)
    {
        var windowStart = DateTime.UtcNow.Subtract(LockoutWindow);

        return await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(log =>
                log.Action == "TutorPortalLoginFailed"
                && log.ActorRole == "Tutor"
                && log.PagePath == "/tutor-portal"
                && log.Success == false
                && log.ActorUsername == username
                && log.OccurredAtUtc >= windowStart)
            .CountAsync();
    }

    public async Task<TutorPortalProfileView?> GetTutorProfileAsync(int tutorOnboardingId)
    {
        var tutor = await _db.TutorOnboardings.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tutorOnboardingId);
        if (tutor == null)
        {
            return null;
        }

        return new TutorPortalProfileView
        {
            TutorOnboardingId = tutor.Id,
            TutorDisplayName = $"{tutor.FirstName} {tutor.LastName}".Trim(),
            Email = tutor.Email,
            Phone = tutor.Phone,
            Address = tutor.Address,
            HighestQualification = tutor.HighestQualification,
            TeachingExperience = tutor.TeachingExperience,
            CoursesToBeTaken = tutor.CoursesToBeTaken,
            CourseDuration = tutor.CourseDuration,
            DBSStatus = tutor.DBSStatus,
            RightToWorkStatus = tutor.RightToWorkStatus,
            SafeguardingTrainingStatus = tutor.SafeguardingTrainingStatus,
            ContractType = tutor.ContractType,
            OnboardingStatus = tutor.Status,
            SubmittedDate = tutor.SubmittedDate,
            ApprovedDate = tutor.ApprovedDate
        };
    }

    public async Task<List<TutorPortalStudentView>> GetStudentsUnderCareAsync(int tutorOnboardingId)
    {
        return await _db.StudentTutorAssignments
            .AsNoTracking()
            .Where(assignment => assignment.TutorOnboardingId == tutorOnboardingId)
            .Join(
                _db.StudentOnboardings.AsNoTracking(),
                assignment => assignment.StudentOnboardingId,
                student => student.Id,
                (assignment, student) => new TutorPortalStudentView
                {
                    StudentOnboardingId = student.Id,
                    StudentDisplayName = $"{student.FirstName} {student.LastName}".Trim(),
                    StudentIdentifier = student.StudentIdentifier,
                    AssignedTutorNames = student.AssignedTutorName,
                    DateOfBirth = student.DateOfBirth,
                    Status = student.Status,
                    EHCPStatus = student.EHCPStatus,
                    SENIndicators = student.SENIndicators,
                    PreferredContactMethod = student.PreferredContactMethod,
                    AssignedDate = assignment.AssignedDateUtc
                })
                .OrderByDescending(student => student.AssignedDate ?? DateTime.MinValue)
            .ToListAsync();
    }

    public async Task<List<TutorStudentAttendanceStateView>> GetStudentAttendanceStatesAsync(int tutorOnboardingId, string username)
    {
        if (tutorOnboardingId <= 0 || string.IsNullOrWhiteSpace(username))
        {
            return new List<TutorStudentAttendanceStateView>();
        }

        var studentIds = await _db.StudentTutorAssignments
            .AsNoTracking()
            .Where(x => x.TutorOnboardingId == tutorOnboardingId)
            .Select(x => x.StudentOnboardingId)
            .Distinct()
            .ToListAsync();

        if (studentIds.Count == 0)
        {
            return new List<TutorStudentAttendanceStateView>();
        }

        var attendances = await _db.TutorStudentLectureAttendances
            .AsNoTracking()
            .Where(x =>
                x.TutorOnboardingId == tutorOnboardingId
                && x.Username == username
                && studentIds.Contains(x.StudentOnboardingId))
            .OrderByDescending(x => x.LectureStartUtc)
            .ToListAsync();

        var result = new List<TutorStudentAttendanceStateView>(studentIds.Count);
        foreach (var studentId in studentIds)
        {
            var records = attendances.Where(x => x.StudentOnboardingId == studentId).ToList();
            var active = records.FirstOrDefault(x => !x.IsClosed);
            var latestClosed = records.FirstOrDefault(x => x.IsClosed);

            result.Add(new TutorStudentAttendanceStateView
            {
                StudentOnboardingId = studentId,
                HasActiveLecture = active is not null,
                ActiveLectureStartUtc = active?.LectureStartUtc,
                LastLectureEndUtc = latestClosed?.LectureEndUtc,
                LastDurationMinutes = latestClosed?.DurationMinutes
            });
        }

        return result;
    }

    public async Task<TutorStudentAttendanceActionResult> StartStudentLectureAsync(int tutorOnboardingId, string username, int studentOnboardingId)
    {
        if (tutorOnboardingId <= 0 || string.IsNullOrWhiteSpace(username) || studentOnboardingId <= 0)
        {
            return new TutorStudentAttendanceActionResult
            {
                Success = false,
                Message = "Invalid attendance request."
            };
        }

        var account = await _db.TutorPortalAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TutorOnboardingId == tutorOnboardingId && x.Username == username && x.IsActive);
        if (account == null)
        {
            return new TutorStudentAttendanceActionResult
            {
                Success = false,
                Message = "Tutor account not found."
            };
        }

        var assignment = await _db.StudentTutorAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TutorOnboardingId == tutorOnboardingId && x.StudentOnboardingId == studentOnboardingId);
        if (assignment == null)
        {
            return new TutorStudentAttendanceActionResult
            {
                Success = false,
                Message = "Student is not assigned to this tutor."
            };
        }

        if (assignment.AssignedDateUtc.Date != DateTime.UtcNow.Date)
        {
            return new TutorStudentAttendanceActionResult
            {
                Success = false,
                Message = "kindly check the date"
            };
        }

        var hasAnotherActive = await _db.TutorStudentLectureAttendances
            .AsNoTracking()
            .AnyAsync(x =>
                x.TutorOnboardingId == tutorOnboardingId
                && x.Username == username
                && !x.IsClosed
                && x.StudentOnboardingId != studentOnboardingId);
        if (hasAnotherActive)
        {
            return new TutorStudentAttendanceActionResult
            {
                Success = false,
                Message = "End the current lecture before starting another student lecture."
            };
        }

        var hasActiveForStudent = await _db.TutorStudentLectureAttendances
            .AsNoTracking()
            .AnyAsync(x =>
                x.TutorOnboardingId == tutorOnboardingId
                && x.Username == username
                && x.StudentOnboardingId == studentOnboardingId
                && !x.IsClosed);
        if (hasActiveForStudent)
        {
            return new TutorStudentAttendanceActionResult
            {
                Success = false,
                Message = "Lecture already active for this student."
            };
        }

        var attendance = new TutorStudentLectureAttendanceEntity
        {
            TutorPortalAccountId = account.Id,
            TutorOnboardingId = tutorOnboardingId,
            StudentOnboardingId = studentOnboardingId,
            Username = username,
            LectureStartUtc = DateTime.UtcNow,
            IsClosed = false
        };

        _db.TutorStudentLectureAttendances.Add(attendance);
        await _db.SaveChangesAsync();

        return new TutorStudentAttendanceActionResult
        {
            Success = true,
            Message = "Lecture started. Attendance is in progress.",
            AttendanceId = attendance.Id
        };
    }

    public async Task<TutorStudentAttendanceActionResult> EndStudentLectureAsync(int tutorOnboardingId, string username, int studentOnboardingId)
    {
        if (tutorOnboardingId <= 0 || string.IsNullOrWhiteSpace(username) || studentOnboardingId <= 0)
        {
            return new TutorStudentAttendanceActionResult
            {
                Success = false,
                Message = "Invalid attendance request."
            };
        }

        var attendance = await _db.TutorStudentLectureAttendances
            .Where(x =>
                x.TutorOnboardingId == tutorOnboardingId
                && x.Username == username
                && x.StudentOnboardingId == studentOnboardingId
                && !x.IsClosed)
            .OrderByDescending(x => x.LectureStartUtc)
            .FirstOrDefaultAsync();

        if (attendance == null)
        {
            return new TutorStudentAttendanceActionResult
            {
                Success = false,
                Message = "No active lecture found for this student."
            };
        }

        var endUtc = DateTime.UtcNow;
        attendance.LectureEndUtc = endUtc;
        attendance.DurationMinutes = Math.Max(0, (int)Math.Round((endUtc - attendance.LectureStartUtc).TotalMinutes));
        attendance.IsClosed = true;

        await _db.SaveChangesAsync();

        await UpdateDailyAttendanceSummaryAsync(
            tutorOnboardingId,
            username,
            DateTime.SpecifyKind(endUtc.Date, DateTimeKind.Utc),
            attendance.DurationMinutes ?? 0);

        var tutorDisplayName = await GetTutorDisplayNameAsync(tutorOnboardingId);
        await GenerateMonthlyTimesheetsIfRequiredAsync(tutorOnboardingId, username, tutorDisplayName);

        return new TutorStudentAttendanceActionResult
        {
            Success = true,
            Message = "Lecture ended and attendance recorded.",
            AttendanceId = attendance.Id
        };
    }

    public async Task<List<TutorMonthlyTimesheetView>> GetTutorMonthlyTimesheetsAsync(int tutorOnboardingId)
    {
        var account = await _db.TutorPortalAccounts
            .Include(x => x.TutorOnboarding)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TutorOnboardingId == tutorOnboardingId);

        if (account != null)
        {
            var tutorDisplayName = $"{account.TutorOnboarding.FirstName} {account.TutorOnboarding.LastName}".Trim();
            await GenerateMonthlyTimesheetsIfRequiredAsync(tutorOnboardingId, account.Username, tutorDisplayName);
        }

        return await _db.TutorMonthlyTimesheets
            .AsNoTracking()
            .Where(x => x.TutorOnboardingId == tutorOnboardingId)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .Take(24)
            .Select(x => new TutorMonthlyTimesheetView
            {
                TutorOnboardingId = x.TutorOnboardingId,
                TutorName = x.TutorName,
                Username = x.Username,
                Year = x.Year,
                Month = x.Month,
                SessionCount = x.SessionCount,
                TotalMinutes = x.TotalMinutes,
                GeneratedAtUtc = x.GeneratedAtUtc
            })
            .ToListAsync();
    }

    public async Task<List<TutorMonthlyTimesheetView>> GetAllTutorMonthlyTimesheetsAsync(int take = 120)
    {
        var activeAccounts = await _db.TutorPortalAccounts
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => new { x.TutorOnboardingId, x.Username })
            .ToListAsync();

        foreach (var account in activeAccounts)
        {
            var tutorDisplayName = await GetTutorDisplayNameAsync(account.TutorOnboardingId);
            await GenerateMonthlyTimesheetsIfRequiredAsync(account.TutorOnboardingId, account.Username, tutorDisplayName);
        }

        return await _db.TutorMonthlyTimesheets
            .AsNoTracking()
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ThenBy(x => x.TutorName)
            .Take(take)
            .Select(x => new TutorMonthlyTimesheetView
            {
                TutorOnboardingId = x.TutorOnboardingId,
                TutorName = x.TutorName,
                Username = x.Username,
                Year = x.Year,
                Month = x.Month,
                SessionCount = x.SessionCount,
                TotalMinutes = x.TotalMinutes,
                GeneratedAtUtc = x.GeneratedAtUtc
            })
            .ToListAsync();
    }

    public async Task<TutorMonthlyTimesheetView?> GetTutorMonthlyTimesheetAsync(int tutorOnboardingId, int year, int month)
    {
        var timesheet = await _db.TutorMonthlyTimesheets
            .AsNoTracking()
            .Where(x => x.TutorOnboardingId == tutorOnboardingId && x.Year == year && x.Month == month)
            .Select(x => new TutorMonthlyTimesheetView
            {
                TutorOnboardingId = x.TutorOnboardingId,
                TutorName = x.TutorName,
                Username = x.Username,
                Year = x.Year,
                Month = x.Month,
                SessionCount = x.SessionCount,
                TotalMinutes = x.TotalMinutes,
                GeneratedAtUtc = x.GeneratedAtUtc
            })
            .FirstOrDefaultAsync();

        return timesheet;
    }

    public async Task<List<TutorMonthlyTimesheetDayView>> GetTutorMonthlyTimesheetDailyDetailsAsync(int tutorOnboardingId, string username, int year, int month)
    {
        return await _db.TutorDailyAttendanceSummaries
            .AsNoTracking()
            .Where(x => x.TutorOnboardingId == tutorOnboardingId
                && x.Username == username
                && x.AttendanceDateUtc.Year == year
                && x.AttendanceDateUtc.Month == month)
            .OrderByDescending(x => x.AttendanceDateUtc)
            .Select(x => new TutorMonthlyTimesheetDayView
            {
                AttendanceDateUtc = x.AttendanceDateUtc,
                SessionCount = x.SessionCount,
                TotalMinutes = x.TotalMinutes,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync();
    }

    public async Task<List<TutorMonthlyTimesheetDetailGroupData>> GetTutorMonthlyTimesheetDailyDetailsBatchAsync(IReadOnlyList<TutorMonthlyTimesheetView> timesheets)
    {
        var selected = timesheets.ToList();

        if (selected.Count == 0)
        {
            return new List<TutorMonthlyTimesheetDetailGroupData>();
        }

        var tutorIds = selected
            .Select(x => x.TutorOnboardingId)
            .Distinct()
            .ToList();

        var usernames = selected
            .Select(x => x.Username)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var monthStartsUtc = selected
            .Select(x => new DateTime(x.Year, x.Month, 1, 0, 0, 0, DateTimeKind.Utc))
            .ToList();

        var minMonthStartUtc = monthStartsUtc.Min();
        var maxMonthEndUtc = monthStartsUtc.Max().AddMonths(1);

        var dailyRows = await _db.TutorDailyAttendanceSummaries
            .AsNoTracking()
            .Where(x => tutorIds.Contains(x.TutorOnboardingId)
                && usernames.Contains(x.Username)
                && x.AttendanceDateUtc >= minMonthStartUtc
                && x.AttendanceDateUtc < maxMonthEndUtc)
            .Select(x => new
            {
                x.TutorOnboardingId,
                x.Username,
                Year = x.AttendanceDateUtc.Year,
                Month = x.AttendanceDateUtc.Month,
                Detail = new TutorMonthlyTimesheetDayView
                {
                    AttendanceDateUtc = x.AttendanceDateUtc,
                    SessionCount = x.SessionCount,
                    TotalMinutes = x.TotalMinutes,
                    UpdatedAtUtc = x.UpdatedAtUtc
                }
            })
            .ToListAsync();

        var groupedLookup = dailyRows
            .GroupBy(x => BuildGroupKey(x.TutorOnboardingId, x.Username, x.Year, x.Month))
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(x => x.Detail)
                    .OrderByDescending(x => x.AttendanceDateUtc)
                    .ToList(),
                StringComparer.Ordinal);

        return selected
            .Select(item =>
            {
                var key = BuildGroupKey(item.TutorOnboardingId, item.Username, item.Year, item.Month);
                groupedLookup.TryGetValue(key, out var details);

                return new TutorMonthlyTimesheetDetailGroupData
                {
                    TutorOnboardingId = item.TutorOnboardingId,
                    Username = item.Username,
                    Year = item.Year,
                    Month = item.Month,
                    Details = details ?? new List<TutorMonthlyTimesheetDayView>()
                };
            })
            .ToList();
    }

    public async Task<(bool Success, string Message)> SubmitTimeOffBookingAsync(int tutorOnboardingId, string username, TutorTimeOffBookingCreateModel booking)
    {
        if (tutorOnboardingId <= 0 || string.IsNullOrWhiteSpace(username))
        {
            return (false, "Tutor session is invalid. Please sign in again.");
        }

        if (booking is null)
        {
            return (false, "Booking request is required.");
        }

        var requestType = NormalizeRequestType(booking.RequestType);
        var startDate = booking.StartDate.Date;
        var endDate = booking.EndDate.Date;

        if (startDate == default || endDate == default)
        {
            return (false, "Start date and end date are required.");
        }

        if (endDate < startDate)
        {
            return (false, "End date cannot be earlier than start date.");
        }

        var reason = booking.Reason?.Trim() ?? string.Empty;
        if (reason.Length < 5)
        {
            return (false, "Please provide a brief reason (at least 5 characters).");
        }

        var accountExists = await _db.TutorPortalAccounts
            .AsNoTracking()
            .AnyAsync(x => x.TutorOnboardingId == tutorOnboardingId && x.Username == username && x.IsActive);

        if (!accountExists)
        {
            return (false, "Tutor account not found.");
        }

        var hasOverlap = await HasOverlappingTimeOffBookingAsync(tutorOnboardingId, username, startDate, endDate);
        if (hasOverlap)
        {
            return (false, "This holiday/off-day period overlaps an existing pending or approved request.");
        }

        var payload = new TutorTimeOffBookingPayload
        {
            TutorOnboardingId = tutorOnboardingId,
            Username = username,
            RequestType = requestType,
            StartDate = startDate,
            EndDate = endDate,
            TotalDays = (endDate - startDate).Days + 1,
            Reason = reason
        };

        _db.ApplicationAuditLogs.Add(new ApplicationAuditLogEntity
        {
            OccurredAtUtc = DateTime.UtcNow,
            EventType = "TutorSchedule",
            Action = TutorTimeOffAction,
            PagePath = TutorPortalPagePath,
            ActorRole = "Tutor",
            ActorUsername = username,
            EntityType = "TutorOnboarding",
            EntityId = tutorOnboardingId.ToString(),
            Success = true,
            Details = JsonSerializer.Serialize(payload)
        });

        await _db.SaveChangesAsync();

        return (true, "Time-off request submitted successfully.");
    }

    public async Task<List<TutorTimeOffBookingView>> GetTimeOffBookingsAsync(int tutorOnboardingId, string username, int take = 30)
    {
        if (tutorOnboardingId <= 0 || string.IsNullOrWhiteSpace(username))
        {
            return new List<TutorTimeOffBookingView>();
        }

        var safeTake = take < 1 ? 1 : Math.Min(take, 100);

        var logs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == TutorTimeOffAction
                && x.PagePath == TutorPortalPagePath
                && x.ActorRole == "Tutor"
                && x.ActorUsername == username
                && x.EntityType == "TutorOnboarding"
                && x.EntityId == tutorOnboardingId.ToString())
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(safeTake)
            .ToListAsync();

        var requestIds = logs.Select(x => x.Id).ToList();
        var requestIdStrings = requestIds.Select(x => x.ToString()).ToList();

        var reviewLogs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == TutorTimeOffReviewAction
                && x.EntityType == "ApplicationAuditLog"
                && x.EntityId != null
                && requestIdStrings.Contains(x.EntityId))
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync();

        var reviewMap = new Dictionary<long, TutorTimeOffReviewPayload>();
        foreach (var reviewLog in reviewLogs)
        {
            if (!long.TryParse(reviewLog.EntityId, out var requestLogId) || reviewMap.ContainsKey(requestLogId))
            {
                continue;
            }

            var payload = TryDeserializeReview(reviewLog.Details);
            if (payload is null)
            {
                continue;
            }

            reviewMap[requestLogId] = payload;
        }

        var items = new List<TutorTimeOffBookingView>(logs.Count);
        foreach (var log in logs)
        {
            var payload = TryDeserializeBooking(log.Details);
            if (payload is null)
            {
                continue;
            }

            reviewMap.TryGetValue(log.Id, out var review);

            items.Add(new TutorTimeOffBookingView
            {
                RequestLogId = log.Id,
                RequestedAtUtc = log.OccurredAtUtc,
                RequestType = NormalizeRequestType(payload.RequestType),
                StartDate = payload.StartDate.Date,
                EndDate = payload.EndDate.Date,
                TotalDays = payload.TotalDays > 0 ? payload.TotalDays : (payload.EndDate.Date - payload.StartDate.Date).Days + 1,
                Reason = payload.Reason,
                ApprovalStatus = NormalizeApprovalStatus(review?.Status),
                ReviewedAtUtc = review?.ReviewedAtUtc,
                ReviewedBy = review?.ReviewedBy,
                ReviewNotes = review?.ReviewNotes
            });
        }

        return items;
    }

    public async Task<List<TutorTimeOffApprovalQueueItem>> GetTimeOffApprovalQueueAsync(int take = 120)
    {
        var safeTake = take < 1 ? 1 : Math.Min(take, 400);

        var bookingLogs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == TutorTimeOffAction
                && x.PagePath == TutorPortalPagePath
                && x.ActorRole == "Tutor"
                && x.EntityType == "TutorOnboarding")
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(safeTake)
            .ToListAsync();

        if (bookingLogs.Count == 0)
        {
            return new List<TutorTimeOffApprovalQueueItem>();
        }

        var tutorIds = bookingLogs
            .Select(x => x.EntityId)
            .Where(x => int.TryParse(x, out _))
            .Select(x => int.Parse(x!))
            .Distinct()
            .ToList();

        var tutorNameMap = await _db.TutorOnboardings
            .AsNoTracking()
            .Where(x => tutorIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => $"{x.FirstName} {x.LastName}".Trim());

        var requestIdStrings = bookingLogs.Select(x => x.Id.ToString()).ToList();
        var reviewLogs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == TutorTimeOffReviewAction
                && x.EntityType == "ApplicationAuditLog"
                && x.EntityId != null
                && requestIdStrings.Contains(x.EntityId))
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync();

        var reviewMap = new Dictionary<long, TutorTimeOffReviewPayload>();
        foreach (var reviewLog in reviewLogs)
        {
            if (!long.TryParse(reviewLog.EntityId, out var requestLogId) || reviewMap.ContainsKey(requestLogId))
            {
                continue;
            }

            var payload = TryDeserializeReview(reviewLog.Details);
            if (payload is null)
            {
                continue;
            }

            reviewMap[requestLogId] = payload;
        }

        var items = new List<TutorTimeOffApprovalQueueItem>(bookingLogs.Count);
        foreach (var bookingLog in bookingLogs)
        {
            var booking = TryDeserializeBooking(bookingLog.Details);
            if (booking is null)
            {
                continue;
            }

            var tutorId = int.TryParse(bookingLog.EntityId, out var parsedTutorId)
                ? parsedTutorId
                : booking.TutorOnboardingId;

            var tutorName = tutorNameMap.TryGetValue(tutorId, out var resolvedName)
                ? resolvedName
                : "Tutor";

            reviewMap.TryGetValue(bookingLog.Id, out var review);

            items.Add(new TutorTimeOffApprovalQueueItem
            {
                RequestLogId = bookingLog.Id,
                RequestedAtUtc = bookingLog.OccurredAtUtc,
                TutorOnboardingId = tutorId,
                TutorName = tutorName,
                Username = booking.Username,
                RequestType = NormalizeRequestType(booking.RequestType),
                StartDate = booking.StartDate.Date,
                EndDate = booking.EndDate.Date,
                TotalDays = booking.TotalDays > 0 ? booking.TotalDays : (booking.EndDate.Date - booking.StartDate.Date).Days + 1,
                Reason = booking.Reason,
                ApprovalStatus = NormalizeApprovalStatus(review?.Status),
                ReviewedAtUtc = review?.ReviewedAtUtc,
                ReviewedBy = review?.ReviewedBy,
                ReviewNotes = review?.ReviewNotes
            });
        }

        return items;
    }

    public async Task<(bool Success, string Message)> UpdateTimeOffRequestStatusAsync(long requestLogId, string reviewedBy, string status, string? reviewNotes)
    {
        if (requestLogId <= 0)
        {
            return (false, "A valid request must be selected.");
        }

        var normalizedStatus = NormalizeApprovalStatus(status);
        if (!string.Equals(normalizedStatus, "Approved", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(normalizedStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Status must be Approved or Rejected.");
        }

        var requestLog = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == requestLogId && x.Action == TutorTimeOffAction);

        if (requestLog is null)
        {
            return (false, "The selected booking request could not be found.");
        }

        var payload = new TutorTimeOffReviewPayload
        {
            RequestLogId = requestLogId,
            Status = normalizedStatus,
            ReviewedBy = string.IsNullOrWhiteSpace(reviewedBy) ? "centre-admin" : reviewedBy.Trim(),
            ReviewedAtUtc = DateTime.UtcNow,
            ReviewNotes = string.IsNullOrWhiteSpace(reviewNotes) ? null : reviewNotes.Trim()
        };

        _db.ApplicationAuditLogs.Add(new ApplicationAuditLogEntity
        {
            OccurredAtUtc = payload.ReviewedAtUtc,
            EventType = "TutorSchedule",
            Action = TutorTimeOffReviewAction,
            PagePath = CentreAdminPagePath,
            ActorRole = "CentreAdmin",
            ActorUsername = payload.ReviewedBy,
            EntityType = "ApplicationAuditLog",
            EntityId = requestLogId.ToString(),
            Success = true,
            Details = JsonSerializer.Serialize(payload)
        });

        await _db.SaveChangesAsync();
        return (true, $"Request marked as {normalizedStatus}.");
    }

    private static string BuildGroupKey(int tutorOnboardingId, string username, int year, int month)
        => $"{tutorOnboardingId}|{username}|{year}|{month}";

    private static string NormalizeRequestType(string? requestType)
    {
        if (string.Equals(requestType, "OffDay", StringComparison.OrdinalIgnoreCase)
            || string.Equals(requestType, "Off Day", StringComparison.OrdinalIgnoreCase)
            || string.Equals(requestType, "Off-Day", StringComparison.OrdinalIgnoreCase))
        {
            return "Off Day";
        }

        return "Holiday";
    }

    private static string NormalizeApprovalStatus(string? status)
    {
        if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
        {
            return "Approved";
        }

        if (string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return "Rejected";
        }

        return "Pending";
    }

    private static TutorTimeOffBookingPayload? TryDeserializeBooking(string? details)
    {
        if (string.IsNullOrWhiteSpace(details))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TutorTimeOffBookingPayload>(details);
        }
        catch
        {
            return null;
        }
    }

    private static TutorTimeOffReviewPayload? TryDeserializeReview(string? details)
    {
        if (string.IsNullOrWhiteSpace(details))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TutorTimeOffReviewPayload>(details);
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> HasOverlappingTimeOffBookingAsync(int tutorOnboardingId, string username, DateTime startDate, DateTime endDate)
    {
        var bookingLogs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == TutorTimeOffAction
                && x.PagePath == TutorPortalPagePath
                && x.ActorRole == "Tutor"
                && x.ActorUsername == username
                && x.EntityType == "TutorOnboarding"
                && x.EntityId == tutorOnboardingId.ToString())
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync();

        if (bookingLogs.Count == 0)
        {
            return false;
        }

        var requestIdStrings = bookingLogs.Select(x => x.Id.ToString()).ToList();

        var reviewLogs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == TutorTimeOffReviewAction
                && x.EntityType == "ApplicationAuditLog"
                && x.EntityId != null
                && requestIdStrings.Contains(x.EntityId))
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync();

        var reviewMap = new Dictionary<long, TutorTimeOffReviewPayload>();
        foreach (var reviewLog in reviewLogs)
        {
            if (!long.TryParse(reviewLog.EntityId, out var requestLogId) || reviewMap.ContainsKey(requestLogId))
            {
                continue;
            }

            var reviewPayload = TryDeserializeReview(reviewLog.Details);
            if (reviewPayload is null)
            {
                continue;
            }

            reviewMap[requestLogId] = reviewPayload;
        }

        foreach (var bookingLog in bookingLogs)
        {
            var bookingPayload = TryDeserializeBooking(bookingLog.Details);
            if (bookingPayload is null)
            {
                continue;
            }

            var bookingStartDate = bookingPayload.StartDate.Date;
            var bookingEndDate = bookingPayload.EndDate.Date;
            var isDateOverlap = bookingStartDate <= endDate && startDate <= bookingEndDate;
            if (!isDateOverlap)
            {
                continue;
            }

            reviewMap.TryGetValue(bookingLog.Id, out var reviewPayload);
            var status = NormalizeApprovalStatus(reviewPayload?.Status);
            if (!string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private async Task GenerateMonthlyTimesheetsIfRequiredAsync(int tutorOnboardingId, string username, string tutorDisplayName)
    {
        var utcNow = DateTime.UtcNow;

        var dailySummaries = await _db.TutorDailyAttendanceSummaries
            .AsNoTracking()
            .Where(x => x.TutorOnboardingId == tutorOnboardingId && x.Username == username)
            .ToListAsync();

        var grouped = dailySummaries
            .GroupBy(x => new { Year = x.AttendanceDateUtc.Year, Month = x.AttendanceDateUtc.Month })
            .Where(group =>
                group.Key.Year < utcNow.Year
                || (group.Key.Year == utcNow.Year && group.Key.Month <= utcNow.Month))
            .ToList();

        if (grouped.Count == 0)
        {
            return;
        }

        var existing = await _db.TutorMonthlyTimesheets
            .Where(x => x.TutorOnboardingId == tutorOnboardingId)
            .ToListAsync();

        foreach (var monthGroup in grouped)
        {
            var totalMinutes = monthGroup.Sum(x => x.TotalMinutes);
            var sessionCount = monthGroup.Sum(x => x.SessionCount);

            var monthEntry = existing.FirstOrDefault(x => x.Year == monthGroup.Key.Year && x.Month == monthGroup.Key.Month);
            if (monthEntry == null)
            {
                _db.TutorMonthlyTimesheets.Add(new TutorMonthlyTimesheetEntity
                {
                    TutorOnboardingId = tutorOnboardingId,
                    TutorName = tutorDisplayName,
                    Username = username,
                    Year = monthGroup.Key.Year,
                    Month = monthGroup.Key.Month,
                    TotalMinutes = totalMinutes,
                    SessionCount = sessionCount,
                    GeneratedAtUtc = utcNow
                });
            }
            else
            {
                monthEntry.TutorName = tutorDisplayName;
                monthEntry.Username = username;
                monthEntry.TotalMinutes = totalMinutes;
                monthEntry.SessionCount = sessionCount;
                monthEntry.GeneratedAtUtc = utcNow;
            }
        }

        await _db.SaveChangesAsync();
    }

    private async Task UpdateDailyAttendanceSummaryAsync(int tutorOnboardingId, string username, DateTime attendanceDateUtc, int durationMinutes)
    {
        var normalizedDateUtc = DateTime.SpecifyKind(attendanceDateUtc.Date, DateTimeKind.Utc);
        var dailySummary = await _db.TutorDailyAttendanceSummaries
            .FirstOrDefaultAsync(x =>
                x.TutorOnboardingId == tutorOnboardingId
                && x.Username == username
                && x.AttendanceDateUtc == normalizedDateUtc);

        if (dailySummary == null)
        {
            _db.TutorDailyAttendanceSummaries.Add(new TutorDailyAttendanceSummaryEntity
            {
                TutorOnboardingId = tutorOnboardingId,
                Username = username,
                AttendanceDateUtc = normalizedDateUtc,
                TotalMinutes = Math.Max(0, durationMinutes),
                SessionCount = 1,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            dailySummary.TotalMinutes += Math.Max(0, durationMinutes);
            dailySummary.SessionCount += 1;
            dailySummary.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    private async Task<string> GetTutorDisplayNameAsync(int tutorOnboardingId)
    {
        var tutor = await _db.TutorOnboardings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tutorOnboardingId);

        if (tutor == null)
        {
            return "Tutor";
        }

        return $"{tutor.FirstName} {tutor.LastName}".Trim();
    }

    private static void CreatePasswordHash(string password, out string hash, out string salt)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 100_000, HashAlgorithmName.SHA256, 32);
        hash = Convert.ToBase64String(hashBytes);
        salt = Convert.ToBase64String(saltBytes);
    }

    private static bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 100_000, HashAlgorithmName.SHA256, 32);
        return string.Equals(Convert.ToBase64String(hashBytes), storedHash, StringComparison.Ordinal);
    }

    private sealed class TutorTimeOffBookingPayload
    {
        public int TutorOnboardingId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    private sealed class TutorTimeOffReviewPayload
    {
        public long RequestLogId { get; set; }
        public string Status { get; set; } = "Pending";
        public string ReviewedBy { get; set; } = string.Empty;
        public DateTime ReviewedAtUtc { get; set; }
        public string? ReviewNotes { get; set; }
    }
}