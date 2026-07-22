using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StudyMgt.Data;
using StudyMgt.Data.Entities;

namespace StudyMgt.Services;

public class CarerPortalSession
{
    public bool IsAuthenticated { get; private set; }
    public string? CarerId { get; private set; }
    public string? CarerName { get; private set; }
    public string? Username { get; private set; }

    public void SignIn(string carerId, string carerName, string username)
    {
        IsAuthenticated = true;
        CarerId = carerId;
        CarerName = carerName;
        Username = username;
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        CarerId = null;
        CarerName = null;
        Username = null;
    }
}

public class CarerPortalStudentView
{
    public string StudentDisplayName { get; set; } = string.Empty;
    public string? StudentIdentifier { get; set; }
    public string Relationship { get; set; } = string.Empty;
    public string CarerStatus { get; set; } = string.Empty;
    public string? StudentOnboardingStatus { get; set; }
    public string? AssignedTutorName { get; set; }
}

public class CarerPortalAuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CarerId { get; set; }
    public string? CarerName { get; set; }
    public string? Username { get; set; }
}

public class CarerPersonalProfileView
{
    public string CarerId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
}

public class CarerManagedStudentView
{
    public int CarerOnboardingId { get; set; }
    public string? StudentIdentifier { get; set; }
    public string StudentFirstName { get; set; } = string.Empty;
    public string StudentLastName { get; set; } = string.Empty;
    public DateTime StudentDateOfBirth { get; set; }
    public string? Relationship { get; set; }
    public string? EHCPStatus { get; set; }
    public string CarerStatus { get; set; } = string.Empty;
    public string? StudentOnboardingStatus { get; set; }
    public string? AssignedTutorName { get; set; }
}

public class CarerStudentTimeOffBookingCreateModel
{
    public int CarerOnboardingId { get; set; }

    public string? StudentIdentifier { get; set; }

    public string? StudentDisplayName { get; set; }

    public string RequestType { get; set; } = "Holiday";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CarerStudentTimeOffBookingView
{
    public long RequestLogId { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public string StudentIdentifier { get; set; } = string.Empty;
    public string StudentDisplayName { get; set; } = string.Empty;
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

public class CarerStudentTimeOffApprovalQueueItem
{
    public long RequestLogId { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public string CarerId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string StudentIdentifier { get; set; } = string.Empty;
    public string StudentDisplayName { get; set; } = string.Empty;
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

public class CarerPortalService
{
    private const string CarerStudentTimeOffAction = "CarerStudentTimeOffBooked";
    private const string CarerStudentTimeOffReviewAction = "CarerStudentTimeOffReviewed";
    private const string CarerPortalPagePath = "/carer-portal";
    private const string CentreAdminPagePath = "/centre-administrators";

    private readonly StudyMgtDbContext _db;

    public CarerPortalService(StudyMgtDbContext db)
    {
        _db = db;
    }

    public async Task<CarerPortalAuthResult> RegisterAsync(string carerId, string email, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new CarerPortalAuthResult { Success = false, Message = "Username and password are required." };
        }

        if (password.Length < 8)
        {
            return new CarerPortalAuthResult { Success = false, Message = "Password must be at least 8 characters." };
        }

        CarerOnboardingEntity? carer;
        var normalizedCarerId = carerId.Trim();
        var normalizedEmail = email.Trim();

        if (!string.IsNullOrWhiteSpace(normalizedCarerId) && !string.IsNullOrWhiteSpace(normalizedEmail))
        {
            carer = await _db.CarerOnboardings
                .FirstOrDefaultAsync(c => c.CarerId == normalizedCarerId && c.Email == normalizedEmail);
            if (carer == null)
            {
                return new CarerPortalAuthResult { Success = false, Message = "Carer record not found for the supplied Carer ID and email." };
            }
        }
        else
        {
            var eligibleCarers = await _db.CarerOnboardings
                .Where(c => c.Status == OnboardingStatus.Approved.ToString())
                .Where(c => !_db.CarerPortalAccounts.Any(a => a.CarerOnboardingId == c.Id))
                .OrderByDescending(c => c.ApprovedAt)
                .ThenBy(c => c.Id)
                .Take(2)
                .ToListAsync();

            if (eligibleCarers.Count == 0)
            {
                return new CarerPortalAuthResult
                {
                    Success = false,
                    Message = "No approved carer record is currently available for account creation."
                };
            }

            if (eligibleCarers.Count > 1)
            {
                return new CarerPortalAuthResult
                {
                    Success = false,
                    Message = "Multiple approved carers are available. Please contact the centre administrator to activate your portal account."
                };
            }

            carer = eligibleCarers[0];
        }

        if (!string.Equals(carer.Status, OnboardingStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return new CarerPortalAuthResult { Success = false, Message = "Only approved carers can create a portal login." };
        }

        var usernameTaken = await _db.CarerPortalAccounts.AnyAsync(a => a.Username == username);
        if (usernameTaken)
        {
            return new CarerPortalAuthResult { Success = false, Message = "Username is already taken." };
        }

        var existingAccount = await _db.CarerPortalAccounts.AnyAsync(a => a.CarerOnboardingId == carer.Id);
        if (existingAccount)
        {
            return new CarerPortalAuthResult { Success = false, Message = "An account already exists for this carer." };
        }

        CreatePasswordHash(password, out var hash, out var salt);

        _db.CarerPortalAccounts.Add(new CarerPortalAccountEntity
        {
            CarerOnboardingId = carer.Id,
            Username = username,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return new CarerPortalAuthResult
        {
            Success = true,
            Message = "Account created successfully.",
            CarerId = carer.CarerId,
            CarerName = $"{carer.FirstName} {carer.LastName}".Trim(),
            Username = username
        };
    }

    public async Task<CarerPortalAuthResult> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new CarerPortalAuthResult { Success = false, Message = "Username and password are required." };
        }

        var account = await _db.CarerPortalAccounts
            .Include(a => a.CarerOnboarding)
            .FirstOrDefaultAsync(a => a.Username == username && a.IsActive);

        if (account == null)
        {
            return new CarerPortalAuthResult { Success = false, Message = "Invalid username or password." };
        }

        if (!string.Equals(account.CarerOnboarding.Status, OnboardingStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return new CarerPortalAuthResult { Success = false, Message = "Your onboarding is not approved yet. Please contact the centre." };
        }

        if (!VerifyPasswordHash(password, account.PasswordHash, account.PasswordSalt))
        {
            return new CarerPortalAuthResult { Success = false, Message = "Invalid username or password." };
        }

        account.LastLoginAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new CarerPortalAuthResult
        {
            Success = true,
            Message = "Login successful.",
            CarerId = account.CarerOnboarding.CarerId,
            CarerName = $"{account.CarerOnboarding.FirstName} {account.CarerOnboarding.LastName}".Trim(),
            Username = account.Username
        };
    }

    public async Task<CarerPortalAuthResult> ResetPasswordAsync(string username, string email, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
        {
            return new CarerPortalAuthResult
            {
                Success = false,
                Message = "Username, onboarding email, and new password are required."
            };
        }

        if (newPassword.Length < 8)
        {
            return new CarerPortalAuthResult
            {
                Success = false,
                Message = "New password must be at least 8 characters."
            };
        }

        var normalizedUsername = username.Trim();
        var normalizedEmail = email.Trim();

        var account = await _db.CarerPortalAccounts
            .Include(a => a.CarerOnboarding)
            .FirstOrDefaultAsync(a => a.Username == normalizedUsername && a.IsActive);

        if (account == null)
        {
            return new CarerPortalAuthResult { Success = false, Message = "Invalid reset details." };
        }

        var onboardingEmail = account.CarerOnboarding.Email?.Trim() ?? string.Empty;
        if (!string.Equals(onboardingEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            return new CarerPortalAuthResult { Success = false, Message = "Invalid reset details." };
        }

        if (!string.Equals(account.CarerOnboarding.Status, OnboardingStatus.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return new CarerPortalAuthResult
            {
                Success = false,
                Message = "Your onboarding is not approved yet. Please contact the centre."
            };
        }

        CreatePasswordHash(newPassword, out var hash, out var salt);
        account.PasswordHash = hash;
        account.PasswordSalt = salt;
        await _db.SaveChangesAsync();

        return new CarerPortalAuthResult
        {
            Success = true,
            Message = "Password reset successful. You can now sign in with your new password.",
            CarerId = account.CarerOnboarding.CarerId,
            CarerName = $"{account.CarerOnboarding.FirstName} {account.CarerOnboarding.LastName}".Trim(),
            Username = account.Username
        };
    }

    public async Task<List<CarerPortalStudentView>> GetStudentsUnderCareAsync(string carerId)
    {
        var carers = await _db.CarerOnboardings
            .AsNoTracking()
            .Where(c => c.CarerId == carerId)
            .ToListAsync();

        var studentIdentifiers = carers
            .Select(c => c.StudentId)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!)
            .Distinct()
            .ToList();

        var students = await _db.StudentOnboardings
            .AsNoTracking()
            .Where(s => studentIdentifiers.Contains(s.StudentIdentifier!))
            .ToListAsync();

        var result = new List<CarerPortalStudentView>();

        foreach (var carer in carers)
        {
            var student = students.FirstOrDefault(s => s.StudentIdentifier == carer.StudentId);
            result.Add(new CarerPortalStudentView
            {
                StudentDisplayName = string.IsNullOrWhiteSpace(carer.StudentName)
                    ? $"{student?.FirstName} {student?.LastName}".Trim()
                    : carer.StudentName!,
                StudentIdentifier = carer.StudentId,
                Relationship = carer.Relationship ?? "Carer",
                CarerStatus = carer.Status,
                StudentOnboardingStatus = student?.Status,
                AssignedTutorName = student?.AssignedTutorName
            });
        }

        return result;
    }

    public async Task<CarerPersonalProfileView?> GetPersonalProfileAsync(string carerId)
    {
        var carer = await _db.CarerOnboardings
            .AsNoTracking()
            .Where(c => c.CarerId == carerId)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (carer == null)
        {
            return null;
        }

        return new CarerPersonalProfileView
        {
            CarerId = carer.CarerId,
            FirstName = carer.FirstName ?? string.Empty,
            LastName = carer.LastName ?? string.Empty,
            Email = carer.Email ?? string.Empty,
            PhoneNumber = carer.PhoneNumber,
            Address = carer.Address,
            PreferredContactMethod = carer.PreferredContactMethod,
            EmergencyContactName = carer.EmergencyContactName,
            EmergencyContactPhone = carer.EmergencyContactPhone,
            EmergencyContactRelationship = carer.EmergencyContactRelationship
        };
    }

    public async Task<(bool Success, string Message)> UpdatePersonalProfileAsync(string carerId, CarerPersonalProfileView profile)
    {
        if (string.IsNullOrWhiteSpace(profile.FirstName) || string.IsNullOrWhiteSpace(profile.LastName) || string.IsNullOrWhiteSpace(profile.Email))
        {
            return (false, "First name, last name and email are required.");
        }

        var carers = await _db.CarerOnboardings
            .Where(c => c.CarerId == carerId)
            .ToListAsync();

        if (carers.Count == 0)
        {
            return (false, "Carer record not found.");
        }

        foreach (var carer in carers)
        {
            carer.FirstName = profile.FirstName.Trim();
            carer.LastName = profile.LastName.Trim();
            carer.Email = profile.Email.Trim();
            carer.PhoneNumber = profile.PhoneNumber?.Trim();
            carer.Address = profile.Address?.Trim();
            carer.PreferredContactMethod = profile.PreferredContactMethod?.Trim();
            carer.EmergencyContactName = profile.EmergencyContactName?.Trim();
            carer.EmergencyContactPhone = profile.EmergencyContactPhone?.Trim();
            carer.EmergencyContactRelationship = profile.EmergencyContactRelationship?.Trim();
            carer.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return (true, "Personal information updated successfully.");
    }

    public async Task<List<CarerManagedStudentView>> GetManagedStudentsAsync(string carerId)
    {
        var carers = await _db.CarerOnboardings
            .AsNoTracking()
            .Where(c => c.CarerId == carerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var studentIdentifiers = carers
            .Select(c => c.StudentId)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!)
            .Distinct()
            .ToList();

        var students = await _db.StudentOnboardings
            .AsNoTracking()
            .Where(s => s.StudentIdentifier != null && studentIdentifiers.Contains(s.StudentIdentifier))
            .ToListAsync();

        var result = new List<CarerManagedStudentView>();

        foreach (var carer in carers)
        {
            var student = students.FirstOrDefault(s => s.StudentIdentifier == carer.StudentId);
            var (fallbackFirstName, fallbackLastName) = SplitName(carer.StudentName);

            result.Add(new CarerManagedStudentView
            {
                CarerOnboardingId = carer.Id,
                StudentIdentifier = carer.StudentId,
                StudentFirstName = student?.FirstName ?? fallbackFirstName,
                StudentLastName = student?.LastName ?? fallbackLastName,
                StudentDateOfBirth = student?.DateOfBirth ?? carer.StudentDateOfBirth,
                Relationship = carer.Relationship,
                EHCPStatus = string.IsNullOrWhiteSpace(student?.EHCPStatus) ? carer.EHCPStatus : student!.EHCPStatus,
                CarerStatus = carer.Status,
                StudentOnboardingStatus = student?.Status,
                AssignedTutorName = student?.AssignedTutorName
            });
        }

        return result;
    }

    public async Task<(bool Success, string Message)> UpdateManagedStudentAsync(string carerId, CarerManagedStudentView model)
    {
        if (string.IsNullOrWhiteSpace(model.StudentFirstName) || string.IsNullOrWhiteSpace(model.StudentLastName))
        {
            return (false, "Student first name and last name are required.");
        }

        var carer = await _db.CarerOnboardings
            .FirstOrDefaultAsync(c => c.Id == model.CarerOnboardingId && c.CarerId == carerId);

        if (carer == null)
        {
            return (false, "Assigned student record not found.");
        }

        carer.StudentName = $"{model.StudentFirstName.Trim()} {model.StudentLastName.Trim()}".Trim();
        carer.StudentDateOfBirth = model.StudentDateOfBirth;
        carer.Relationship = model.Relationship?.Trim();
        carer.EHCPStatus = model.EHCPStatus?.Trim();
        carer.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(carer.StudentId))
        {
            var student = await _db.StudentOnboardings
                .FirstOrDefaultAsync(s => s.StudentIdentifier == carer.StudentId);

            if (student != null)
            {
                student.FirstName = model.StudentFirstName.Trim();
                student.LastName = model.StudentLastName.Trim();
                student.DateOfBirth = model.StudentDateOfBirth;
                student.EHCPStatus = model.EHCPStatus?.Trim() ?? string.Empty;
                student.RelationshipToStudent = model.Relationship?.Trim() ?? student.RelationshipToStudent;
            }
        }

        await _db.SaveChangesAsync();
        return (true, "Student information updated successfully.");
    }

    public async Task<(bool Success, string Message)> SubmitStudentTimeOffBookingAsync(string carerId, string username, CarerStudentTimeOffBookingCreateModel booking)
    {
        if (string.IsNullOrWhiteSpace(carerId) || string.IsNullOrWhiteSpace(username))
        {
            return (false, "Carer session is invalid. Please sign in again.");
        }

        if (booking is null)
        {
            return (false, "Booking request is required.");
        }

        if (booking.CarerOnboardingId <= 0)
        {
            return (false, "Select a student for this request.");
        }

        var linkedCarer = await _db.CarerOnboardings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == booking.CarerOnboardingId && x.CarerId == carerId);

        if (linkedCarer is null)
        {
            return (false, "The selected student is not linked to your carer profile.");
        }

        var studentIdentifier = booking.StudentIdentifier?.Trim();
        if (string.IsNullOrWhiteSpace(studentIdentifier))
        {
            studentIdentifier = !string.IsNullOrWhiteSpace(linkedCarer.StudentId)
                ? linkedCarer.StudentId.Trim()
                : $"CARER-STUDENT-{linkedCarer.Id}";
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

        var accountExists = await _db.CarerPortalAccounts
            .Include(x => x.CarerOnboarding)
            .AsNoTracking()
            .AnyAsync(x => x.IsActive && x.Username == username && x.CarerOnboarding.CarerId == carerId);

        if (!accountExists)
        {
            return (false, "Carer account not found.");
        }

        var hasOverlap = await HasOverlappingStudentTimeOffBookingAsync(studentIdentifier, startDate, endDate);
        if (hasOverlap)
        {
            return (false, "This holiday/off-day period overlaps an existing pending or approved request for this student.");
        }

        var payload = new CarerStudentTimeOffBookingPayload
        {
            CarerId = carerId,
            Username = username,
            StudentIdentifier = studentIdentifier,
            StudentDisplayName = booking.StudentDisplayName?.Trim()
                ?? linkedCarer.StudentName?.Trim()
                ?? string.Empty,
            RequestType = requestType,
            StartDate = startDate,
            EndDate = endDate,
            TotalDays = (endDate - startDate).Days + 1,
            Reason = reason
        };

        _db.ApplicationAuditLogs.Add(new ApplicationAuditLogEntity
        {
            OccurredAtUtc = DateTime.UtcNow,
            EventType = "CarerStudentSchedule",
            Action = CarerStudentTimeOffAction,
            PagePath = CarerPortalPagePath,
            ActorRole = "Carer",
            ActorUsername = username,
            EntityType = "Student",
            EntityId = studentIdentifier,
            Success = true,
            Details = JsonSerializer.Serialize(payload)
        });

        await _db.SaveChangesAsync();
        return (true, "Student time-off request submitted successfully.");
    }

    public async Task<List<CarerStudentTimeOffBookingView>> GetStudentTimeOffBookingsAsync(string carerId, string username, int take = 40)
    {
        if (string.IsNullOrWhiteSpace(carerId) || string.IsNullOrWhiteSpace(username))
        {
            return new List<CarerStudentTimeOffBookingView>();
        }

        var safeTake = take < 1 ? 1 : Math.Min(take, 200);
        var logs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == CarerStudentTimeOffAction
                && x.PagePath == CarerPortalPagePath
                && x.ActorRole == "Carer"
                && x.ActorUsername == username)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(safeTake)
            .ToListAsync();

        var requestIds = logs.Select(x => x.Id).ToList();
        var requestIdStrings = requestIds.Select(x => x.ToString()).ToList();

        var reviewLogs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == CarerStudentTimeOffReviewAction
                && x.EntityType == "ApplicationAuditLog"
                && x.EntityId != null
                && requestIdStrings.Contains(x.EntityId))
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync();

        var reviewMap = new Dictionary<long, CarerStudentTimeOffReviewPayload>();
        foreach (var reviewLog in reviewLogs)
        {
            if (!long.TryParse(reviewLog.EntityId, out var requestLogId) || reviewMap.ContainsKey(requestLogId))
            {
                continue;
            }

            var payload = TryDeserializeStudentTimeOffReview(reviewLog.Details);
            if (payload is null)
            {
                continue;
            }

            reviewMap[requestLogId] = payload;
        }

        var items = new List<CarerStudentTimeOffBookingView>(logs.Count);
        foreach (var log in logs)
        {
            var payload = TryDeserializeStudentTimeOffBooking(log.Details);
            if (payload is null)
            {
                continue;
            }

            if (!string.Equals(payload.CarerId, carerId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            reviewMap.TryGetValue(log.Id, out var review);

            items.Add(new CarerStudentTimeOffBookingView
            {
                RequestLogId = log.Id,
                RequestedAtUtc = log.OccurredAtUtc,
                StudentIdentifier = payload.StudentIdentifier,
                StudentDisplayName = payload.StudentDisplayName,
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

    public async Task<List<CarerStudentTimeOffApprovalQueueItem>> GetStudentTimeOffApprovalQueueAsync(int take = 150)
    {
        var safeTake = take < 1 ? 1 : Math.Min(take, 400);
        var bookingLogs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == CarerStudentTimeOffAction
                && x.PagePath == CarerPortalPagePath
                && x.ActorRole == "Carer")
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(safeTake)
            .ToListAsync();

        if (bookingLogs.Count == 0)
        {
            return new List<CarerStudentTimeOffApprovalQueueItem>();
        }

        var requestIdStrings = bookingLogs.Select(x => x.Id.ToString()).ToList();
        var reviewLogs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == CarerStudentTimeOffReviewAction
                && x.EntityType == "ApplicationAuditLog"
                && x.EntityId != null
                && requestIdStrings.Contains(x.EntityId))
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync();

        var reviewMap = new Dictionary<long, CarerStudentTimeOffReviewPayload>();
        foreach (var reviewLog in reviewLogs)
        {
            if (!long.TryParse(reviewLog.EntityId, out var requestLogId) || reviewMap.ContainsKey(requestLogId))
            {
                continue;
            }

            var payload = TryDeserializeStudentTimeOffReview(reviewLog.Details);
            if (payload is null)
            {
                continue;
            }

            reviewMap[requestLogId] = payload;
        }

        var items = new List<CarerStudentTimeOffApprovalQueueItem>(bookingLogs.Count);
        foreach (var bookingLog in bookingLogs)
        {
            var payload = TryDeserializeStudentTimeOffBooking(bookingLog.Details);
            if (payload is null)
            {
                continue;
            }

            reviewMap.TryGetValue(bookingLog.Id, out var review);
            items.Add(new CarerStudentTimeOffApprovalQueueItem
            {
                RequestLogId = bookingLog.Id,
                RequestedAtUtc = bookingLog.OccurredAtUtc,
                CarerId = payload.CarerId,
                Username = payload.Username,
                StudentIdentifier = payload.StudentIdentifier,
                StudentDisplayName = payload.StudentDisplayName,
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

    public async Task<(bool Success, string Message)> UpdateStudentTimeOffRequestStatusAsync(long requestLogId, string reviewedBy, string status, string? reviewNotes)
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
            .FirstOrDefaultAsync(x => x.Id == requestLogId && x.Action == CarerStudentTimeOffAction);

        if (requestLog is null)
        {
            return (false, "The selected booking request could not be found.");
        }

        var payload = new CarerStudentTimeOffReviewPayload
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
            EventType = "CarerStudentSchedule",
            Action = CarerStudentTimeOffReviewAction,
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

    private static CarerStudentTimeOffBookingPayload? TryDeserializeStudentTimeOffBooking(string? details)
    {
        if (string.IsNullOrWhiteSpace(details))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CarerStudentTimeOffBookingPayload>(details);
        }
        catch
        {
            return null;
        }
    }

    private static CarerStudentTimeOffReviewPayload? TryDeserializeStudentTimeOffReview(string? details)
    {
        if (string.IsNullOrWhiteSpace(details))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CarerStudentTimeOffReviewPayload>(details);
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> HasOverlappingStudentTimeOffBookingAsync(string studentIdentifier, DateTime startDate, DateTime endDate)
    {
        var bookingLogs = await _db.ApplicationAuditLogs
            .AsNoTracking()
            .Where(x =>
                x.Action == CarerStudentTimeOffAction
                && x.PagePath == CarerPortalPagePath
                && x.ActorRole == "Carer"
                && x.EntityType == "Student"
                && x.EntityId == studentIdentifier)
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
                x.Action == CarerStudentTimeOffReviewAction
                && x.EntityType == "ApplicationAuditLog"
                && x.EntityId != null
                && requestIdStrings.Contains(x.EntityId))
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync();

        var reviewMap = new Dictionary<long, CarerStudentTimeOffReviewPayload>();
        foreach (var reviewLog in reviewLogs)
        {
            if (!long.TryParse(reviewLog.EntityId, out var requestLogId) || reviewMap.ContainsKey(requestLogId))
            {
                continue;
            }

            var reviewPayload = TryDeserializeStudentTimeOffReview(reviewLog.Details);
            if (reviewPayload is null)
            {
                continue;
            }

            reviewMap[requestLogId] = reviewPayload;
        }

        foreach (var bookingLog in bookingLogs)
        {
            var bookingPayload = TryDeserializeStudentTimeOffBooking(bookingLog.Details);
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

    private static (string firstName, string lastName) SplitName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return (string.Empty, string.Empty);
        }

        var parts = displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return (parts[0], string.Empty);
        }

        return (parts[0], string.Join(' ', parts.Skip(1)));
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
        var computed = Convert.ToBase64String(hashBytes);
        return string.Equals(computed, storedHash, StringComparison.Ordinal);
    }

    private sealed class CarerStudentTimeOffBookingPayload
    {
        public string CarerId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string StudentIdentifier { get; set; } = string.Empty;
        public string StudentDisplayName { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    private sealed class CarerStudentTimeOffReviewPayload
    {
        public long RequestLogId { get; set; }
        public string Status { get; set; } = "Pending";
        public string ReviewedBy { get; set; } = string.Empty;
        public DateTime ReviewedAtUtc { get; set; }
        public string? ReviewNotes { get; set; }
    }
}
