namespace StudyMgt.Data.Entities;

public class TutorPortalSessionLogEntity
{
    public int Id { get; set; }
    public int TutorPortalAccountId { get; set; }
    public int TutorOnboardingId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime LoginAtUtc { get; set; }
    public DateTime? LogoutAtUtc { get; set; }
    public int? DurationMinutes { get; set; }
    public bool IsClosed { get; set; }

    public TutorPortalAccountEntity TutorPortalAccount { get; set; } = default!;
}

public class TutorMonthlyTimesheetEntity
{
    public int Id { get; set; }
    public int TutorOnboardingId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalMinutes { get; set; }
    public int SessionCount { get; set; }
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}

public class TutorDailyAttendanceSummaryEntity
{
    public int Id { get; set; }
    public int TutorOnboardingId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime AttendanceDateUtc { get; set; }
    public int TotalMinutes { get; set; }
    public int SessionCount { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class TutorStudentLectureAttendanceEntity
{
    public int Id { get; set; }
    public int TutorPortalAccountId { get; set; }
    public int TutorOnboardingId { get; set; }
    public int StudentOnboardingId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime LectureStartUtc { get; set; }
    public DateTime? LectureEndUtc { get; set; }
    public int? DurationMinutes { get; set; }
    public bool IsClosed { get; set; }

    public TutorPortalAccountEntity TutorPortalAccount { get; set; } = default!;
}
