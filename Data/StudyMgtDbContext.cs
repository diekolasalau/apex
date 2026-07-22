using Microsoft.EntityFrameworkCore;
using StudyMgt.Data.Entities;

namespace StudyMgt.Data;

public class StudyMgtDbContext : DbContext
{
    public StudyMgtDbContext(DbContextOptions<StudyMgtDbContext> options)
        : base(options)
    {
    }

    public DbSet<DbRoundTripLog> DbRoundTripLogs => Set<DbRoundTripLog>();
    public DbSet<ApplicationAuditLogEntity> ApplicationAuditLogs => Set<ApplicationAuditLogEntity>();
    public DbSet<StudentOnboardingEntity> StudentOnboardings => Set<StudentOnboardingEntity>();
    public DbSet<StudentOnboardingAuditEntity> StudentOnboardingAudits => Set<StudentOnboardingAuditEntity>();
    public DbSet<StudentConsentRecordEntity> StudentConsentRecords => Set<StudentConsentRecordEntity>();
    public DbSet<StudentTutorAssignmentEntity> StudentTutorAssignments => Set<StudentTutorAssignmentEntity>();

    public DbSet<TutorOnboardingEntity> TutorOnboardings => Set<TutorOnboardingEntity>();
    public DbSet<TutorOnboardingAuditEntity> TutorOnboardingAudits => Set<TutorOnboardingAuditEntity>();
    public DbSet<TutorPortalAccountEntity> TutorPortalAccounts => Set<TutorPortalAccountEntity>();
    public DbSet<TutorPortalSessionLogEntity> TutorPortalSessionLogs => Set<TutorPortalSessionLogEntity>();
    public DbSet<TutorDailyAttendanceSummaryEntity> TutorDailyAttendanceSummaries => Set<TutorDailyAttendanceSummaryEntity>();
    public DbSet<TutorMonthlyTimesheetEntity> TutorMonthlyTimesheets => Set<TutorMonthlyTimesheetEntity>();
    public DbSet<TutorStudentLectureAttendanceEntity> TutorStudentLectureAttendances => Set<TutorStudentLectureAttendanceEntity>();

    public DbSet<CarerOnboardingEntity> CarerOnboardings => Set<CarerOnboardingEntity>();
    public DbSet<CarerConsentAuditEntryEntity> CarerConsentAuditEntries => Set<CarerConsentAuditEntryEntity>();
    public DbSet<CarerPortalAccountEntity> CarerPortalAccounts => Set<CarerPortalAccountEntity>();

    public DbSet<CentreAdminPortalAccountEntity> CentreAdminPortalAccounts => Set<CentreAdminPortalAccountEntity>();
    public DbSet<CentreAdminPasswordResetTokenEntity> CentreAdminPasswordResetTokens => Set<CentreAdminPasswordResetTokenEntity>();
    public DbSet<CouncilPortalAccountEntity> CouncilPortalAccounts => Set<CouncilPortalAccountEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StudentOnboardingEntity>(entity =>
        {
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.SubmittedDate);

            entity.Property(x => x.FirstName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100);
            entity.Property(x => x.Gender).HasMaxLength(50);
            entity.Property(x => x.StudentIdentifier).HasMaxLength(50);
            entity.Property(x => x.EmergencyContactName).HasMaxLength(100);
            entity.Property(x => x.EmergencyContactPhone).HasMaxLength(50);
            entity.Property(x => x.EmergencyContactEmail).HasMaxLength(256);
            entity.Property(x => x.RelationshipToStudent).HasMaxLength(100);
            entity.Property(x => x.EHCPStatus).HasMaxLength(100);
            entity.Property(x => x.PreferredContactMethod).HasMaxLength(100);
            entity.Property(x => x.AssignedTutorName).HasMaxLength(200);
            entity.Property(x => x.Status).HasMaxLength(50);
            entity.Property(x => x.ApprovedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<ApplicationAuditLogEntity>(entity =>
        {
            entity.HasIndex(x => x.OccurredAtUtc);
            entity.HasIndex(x => x.EventType);
            entity.HasIndex(x => x.Action);
            entity.HasIndex(x => x.PagePath);

            entity.Property(x => x.EventType).HasMaxLength(50);
            entity.Property(x => x.Action).HasMaxLength(150);
            entity.Property(x => x.PagePath).HasMaxLength(256);
            entity.Property(x => x.ActorRole).HasMaxLength(80);
            entity.Property(x => x.ActorUsername).HasMaxLength(120);
            entity.Property(x => x.EntityType).HasMaxLength(120);
            entity.Property(x => x.EntityId).HasMaxLength(120);
            entity.Property(x => x.Details).HasMaxLength(4000);
        });

        modelBuilder.Entity<StudentOnboardingAuditEntity>(entity =>
        {
            entity.HasIndex(x => x.StudentOnboardingId);
            entity.HasIndex(x => x.ChangedDate);
            entity.Property(x => x.Action).HasMaxLength(100);
            entity.Property(x => x.FieldChanged).HasMaxLength(100);
            entity.Property(x => x.ChangedBy).HasMaxLength(100);

            entity.HasOne(x => x.StudentOnboarding)
                .WithMany(x => x.Audits)
                .HasForeignKey(x => x.StudentOnboardingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudentConsentRecordEntity>(entity =>
        {
            entity.HasIndex(x => x.StudentOnboardingId);
            entity.HasIndex(x => x.RecordedDate);
            entity.Property(x => x.ConsentType).HasMaxLength(100);

            entity.HasOne(x => x.StudentOnboarding)
                .WithMany(x => x.ConsentRecords)
                .HasForeignKey(x => x.StudentOnboardingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

            modelBuilder.Entity<StudentTutorAssignmentEntity>(entity =>
            {
                entity.HasIndex(x => x.StudentOnboardingId);
                entity.HasIndex(x => x.TutorOnboardingId);
                entity.HasIndex(x => new { x.StudentOnboardingId, x.TutorOnboardingId }).IsUnique();
                entity.Property(x => x.TutorName).HasMaxLength(200);

                entity.HasOne(x => x.StudentOnboarding)
                .WithMany(x => x.TutorAssignments)
                .HasForeignKey(x => x.StudentOnboardingId)
                .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<TutorOnboardingEntity>(entity =>
        {
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.SubmittedDate);
            entity.HasIndex(x => x.Email);

            entity.Property(x => x.FirstName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100);
            entity.Property(x => x.Gender).HasMaxLength(50);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.HighestQualification).HasMaxLength(200);
            entity.Property(x => x.CoursesToBeTaken).HasMaxLength(500);
            entity.Property(x => x.CourseDuration).HasMaxLength(100);
            entity.Property(x => x.Reference1Name).HasMaxLength(500);
            entity.Property(x => x.Reference1Contact).HasMaxLength(200);
            entity.Property(x => x.Reference2Name).HasMaxLength(500);
            entity.Property(x => x.Reference2Contact).HasMaxLength(200);
            entity.Property(x => x.DBSStatus).HasMaxLength(100);
            entity.Property(x => x.DBSCertificateNumber).HasMaxLength(50);
            entity.Property(x => x.DBSCheckType).HasMaxLength(50);
            entity.Property(x => x.RightToWorkStatus).HasMaxLength(100);
            entity.Property(x => x.VisaType).HasMaxLength(50);
            entity.Property(x => x.PassportNumber).HasMaxLength(50);
            entity.Property(x => x.SafeguardingTrainingStatus).HasMaxLength(100);
            entity.Property(x => x.TrainingProvider).HasMaxLength(100);
            entity.Property(x => x.ContractType).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(50);
            entity.Property(x => x.ApprovedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<TutorOnboardingAuditEntity>(entity =>
        {
            entity.HasIndex(x => x.TutorOnboardingId);
            entity.HasIndex(x => x.ChangedDate);
            entity.Property(x => x.Action).HasMaxLength(100);
            entity.Property(x => x.FieldChanged).HasMaxLength(100);
            entity.Property(x => x.ChangedBy).HasMaxLength(100);

            entity.HasOne(x => x.TutorOnboarding)
                .WithMany(x => x.Audits)
                .HasForeignKey(x => x.TutorOnboardingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

            modelBuilder.Entity<TutorPortalAccountEntity>(entity =>
            {
                entity.HasIndex(x => x.Username).IsUnique();
                entity.HasIndex(x => x.TutorOnboardingId).IsUnique();
                entity.Property(x => x.Username).HasMaxLength(100);
                entity.Property(x => x.PasswordHash).HasMaxLength(512);
                entity.Property(x => x.PasswordSalt).HasMaxLength(256);

                entity.HasOne(x => x.TutorOnboarding)
                .WithOne()
                .HasForeignKey<TutorPortalAccountEntity>(x => x.TutorOnboardingId)
                .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<TutorPortalSessionLogEntity>(entity =>
        {
            entity.HasIndex(x => x.TutorPortalAccountId);
            entity.HasIndex(x => x.TutorOnboardingId);
            entity.HasIndex(x => x.Username);
            entity.HasIndex(x => x.LoginAtUtc);
            entity.HasIndex(x => x.IsClosed);
            entity.Property(x => x.Username).HasMaxLength(100);

            entity.HasOne(x => x.TutorPortalAccount)
                .WithMany()
                .HasForeignKey(x => x.TutorPortalAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TutorMonthlyTimesheetEntity>(entity =>
        {
            entity.HasIndex(x => new { x.TutorOnboardingId, x.Year, x.Month }).IsUnique();
            entity.HasIndex(x => x.Username);
            entity.Property(x => x.TutorName).HasMaxLength(200);
            entity.Property(x => x.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<TutorDailyAttendanceSummaryEntity>(entity =>
        {
            entity.HasIndex(x => x.TutorOnboardingId);
            entity.HasIndex(x => x.Username);
            entity.HasIndex(x => x.AttendanceDateUtc);
            entity.HasIndex(x => new { x.TutorOnboardingId, x.Username, x.AttendanceDateUtc }).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<TutorStudentLectureAttendanceEntity>(entity =>
        {
            entity.HasIndex(x => x.TutorPortalAccountId);
            entity.HasIndex(x => x.TutorOnboardingId);
            entity.HasIndex(x => x.StudentOnboardingId);
            entity.HasIndex(x => x.Username);
            entity.HasIndex(x => x.LectureStartUtc);
            entity.HasIndex(x => x.IsClosed);
            entity.Property(x => x.Username).HasMaxLength(100);

            entity.HasOne(x => x.TutorPortalAccount)
                .WithMany()
                .HasForeignKey(x => x.TutorPortalAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CarerOnboardingEntity>(entity =>
        {
            entity.HasIndex(x => x.CarerId).IsUnique();
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAt);

            entity.Property(x => x.CarerId).HasMaxLength(32);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(100);
            entity.Property(x => x.FirstName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.PhoneNumber).HasMaxLength(50);
            entity.Property(x => x.StudentId).HasMaxLength(50);
            entity.Property(x => x.StudentName).HasMaxLength(200);
            entity.Property(x => x.Relationship).HasMaxLength(100);
            entity.Property(x => x.EHCPStatus).HasMaxLength(100);
            entity.Property(x => x.PreferredContactMethod).HasMaxLength(100);
            entity.Property(x => x.EmergencyContactName).HasMaxLength(100);
            entity.Property(x => x.EmergencyContactPhone).HasMaxLength(50);
            entity.Property(x => x.EmergencyContactRelationship).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(50);
            entity.Property(x => x.ApprovedByUserId).HasMaxLength(100);
        });

        modelBuilder.Entity<CarerConsentAuditEntryEntity>(entity =>
        {
            entity.HasIndex(x => x.CarerOnboardingId);
            entity.HasIndex(x => x.Timestamp);
            entity.Property(x => x.ConsentType).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(512);

            entity.HasOne(x => x.CarerOnboarding)
                .WithMany(x => x.ConsentAuditEntries)
                .HasForeignKey(x => x.CarerOnboardingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CarerPortalAccountEntity>(entity =>
        {
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.CarerOnboardingId).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(100);
            entity.Property(x => x.PasswordHash).HasMaxLength(512);
            entity.Property(x => x.PasswordSalt).HasMaxLength(256);

            entity.HasOne(x => x.CarerOnboarding)
                .WithOne()
                .HasForeignKey<CarerPortalAccountEntity>(x => x.CarerOnboardingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CouncilPortalAccountEntity>(entity =>
        {
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(100);
            entity.Property(x => x.PasswordHash).HasMaxLength(512);
            entity.Property(x => x.PasswordSalt).HasMaxLength(256);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.DisplayName).HasMaxLength(200);
        });
    }
}
