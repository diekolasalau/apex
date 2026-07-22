using Microsoft.EntityFrameworkCore;
using StudyMgt.Data;
using StudyMgt.Services;

namespace StudyMgt.IntegrationTests;

public class OnboardingPersistenceIntegrationTests
{
    [Fact]
    public async Task Student_Onboarding_Persists_And_Can_Be_Approved_And_Assigned()
    {
        await using var db = await CreateDbContextAsync();
        var service = new StudentOnboardingService(db);

        var save = await service.SaveOnboardingAsync(new StudentOnboardingModel
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            DateOfBirth = new DateTime(2012, 6, 1),
            Gender = "Female",
            StudentId = "STU1001",
            EmergencyContactName = "Mary Lovelace",
            EmergencyContactPhone = "07123456789",
            EmergencyContactEmail = "mary@example.com",
            RelationshipToStudent = "Parent",
            SENIndicators = "Dyslexia support required",
            EHCPStatus = "In Place",
            ILPSummary = "Weekly literacy intervention",
            RequiresCommunicationSupport = true,
            ConsentDataSharing = true,
            ConsentPhotos = false,
            ConsentEmailCommunication = true,
            ConsentSMSCommunication = true,
            PreferredContactMethod = "Email",
            PrivacyNoticeAcknowledged = true,
            DeclarationConfirmed = true
        });

        Assert.True(save.Success);
        Assert.NotNull(save.Data);
        var id = save.Data!.Id;

        var tutorService = new TutorOnboardingService(db);
        var tutorSave = await tutorService.SaveOnboardingAsync(new TutorOnboardingModel
        {
            FirstName = "Tutor",
            LastName = "Turing",
            DateOfBirth = new DateTime(1990, 2, 1),
            Gender = "Male",
            Email = "tutor.turing@example.com",
            Phone = "07999990001",
            HighestQualification = "BEd",
            TeachingExperience = "SEN tutoring",
            Reference1Name = "Grace Hopper",
            Reference1Contact = "grace@example.com",
            DBSStatus = "Completed",
            RightToWorkStatus = "UK Citizen",
            SafeguardingTrainingStatus = "Completed",
            ContractType = "Part-Time",
            ConsentDataProcessing = true,
            ConsentDBSCheck = true,
            ConsentReferences = true,
            PrivacyNoticeAcknowledged = true,
            DeclarationConfirmed = true
        });

        Assert.True(tutorSave.Success);
        Assert.NotNull(tutorSave.Data);
        var tutorId = tutorSave.Data!.Id;

        var tutorApproved = await tutorService.ApproveOnboardingAsync(tutorId, "Tutor approved by test admin");
        var approved = await service.ApproveOnboardingAsync(id, "Verified by test admin");
        var assigned = await service.AssignTutorAsync(id, tutorId: tutorId, tutorName: "Tutor Turing");
        var all = (await service.GetAllOnboardingsAsync()).ToList();
        var fromDb = await service.GetOnboardingByIdAsync(id);
        var audit = await service.GetAuditTrailAsync(id);

        Assert.True(tutorApproved);
        Assert.True(approved);
        Assert.True(assigned);
        Assert.Contains(all, x => x.Id == id);
        Assert.NotNull(fromDb);
        Assert.Equal(OnboardingStatus.Approved, fromDb!.Status);
        Assert.Equal(tutorId, fromDb.AssignedTutorId);
        Assert.Equal("Tutor Turing", fromDb.AssignedTutorName);
        Assert.Equal("Tutor Assigned", audit.Action);

        var consentCount = await db.StudentConsentRecords.CountAsync(x => x.StudentOnboardingId == id);
        Assert.Equal(6, consentCount);
    }

    [Fact]
    public async Task Tutor_Onboarding_Persists_And_Can_Be_Approved()
    {
        await using var db = await CreateDbContextAsync();
        var service = new TutorOnboardingService(db);

        var save = await service.SaveOnboardingAsync(new TutorOnboardingModel
        {
            FirstName = "Alan",
            LastName = "Turing",
            DateOfBirth = new DateTime(1990, 2, 1),
            Gender = "Male",
            Email = "alan.turing@example.com",
            Phone = "07999990000",
            HighestQualification = "MSc Computer Science",
            TeachingExperience = "5 years tutoring GCSE maths",
            Reference1Name = "Grace Hopper",
            Reference1Contact = "grace@example.com",
            DBSStatus = "Completed",
            RightToWorkStatus = "UK Citizen",
            SafeguardingTrainingStatus = "Completed",
            ContractType = "Part-Time",
            ConsentDataProcessing = true,
            ConsentDBSCheck = true,
            ConsentReferences = true,
            PrivacyNoticeAcknowledged = true,
            DeclarationConfirmed = true
        });

        Assert.True(save.Success);
        Assert.NotNull(save.Data);
        var id = save.Data!.Id;

        var approved = await service.ApproveOnboardingAsync(id, "Verified references and DBS");
        var all = (await service.GetAllOnboardingsAsync()).ToList();
        var fromDb = await service.GetOnboardingByIdAsync(id);
        var audit = await service.GetAuditTrailAsync(id);

        Assert.True(approved);
        Assert.Contains(all, x => x.Id == id);
        Assert.NotNull(fromDb);
        Assert.Equal(OnboardingStatus.Approved, fromDb!.Status);
        Assert.Equal("Approved", audit.Action);
    }

    [Fact]
    public async Task Student_Assignment_Is_Rejected_When_Student_Not_Approved()
    {
        await using var db = await CreateDbContextAsync();
        var studentService = new StudentOnboardingService(db);
        var tutorService = new TutorOnboardingService(db);

        var studentSave = await studentService.SaveOnboardingAsync(new StudentOnboardingModel
        {
            FirstName = "Pending",
            LastName = "Student",
            DateOfBirth = new DateTime(2011, 1, 1),
            EmergencyContactName = "Guardian",
            EmergencyContactPhone = "07111111111",
            RelationshipToStudent = "Parent",
            SENIndicators = "Needs literacy support",
            EHCPStatus = "In Place",
            ConsentDataSharing = true,
            ConsentEmailCommunication = true,
            PreferredContactMethod = "Email",
            PrivacyNoticeAcknowledged = true,
            DeclarationConfirmed = true
        });

        var tutorSave = await tutorService.SaveOnboardingAsync(new TutorOnboardingModel
        {
            FirstName = "Approved",
            LastName = "Tutor",
            DateOfBirth = new DateTime(1988, 1, 1),
            Email = "approved.tutor@example.com",
            Phone = "07222222222",
            HighestQualification = "PGCE",
            TeachingExperience = "8 years SEN tutoring",
            Reference1Name = "Ref Person",
            Reference1Contact = "ref@example.com",
            DBSStatus = "Completed",
            RightToWorkStatus = "UK Citizen",
            SafeguardingTrainingStatus = "Completed",
            ContractType = "Part-Time",
            ConsentDataProcessing = true,
            ConsentDBSCheck = true,
            ConsentReferences = true,
            PrivacyNoticeAcknowledged = true,
            DeclarationConfirmed = true
        });

        Assert.True(studentSave.Success);
        Assert.True(tutorSave.Success);
        Assert.NotNull(studentSave.Data);
        Assert.NotNull(tutorSave.Data);

        var tutorApproved = await tutorService.ApproveOnboardingAsync(tutorSave.Data!.Id, "Approved tutor");
        Assert.True(tutorApproved);

        var assigned = await studentService.AssignTutorAsync(studentSave.Data!.Id, tutorSave.Data.Id, "Approved Tutor");
        var fromDb = await studentService.GetOnboardingByIdAsync(studentSave.Data.Id);

        Assert.False(assigned);
        Assert.NotNull(fromDb);
        Assert.Null(fromDb!.AssignedTutorId);
        Assert.Null(fromDb.AssignedTutorName);
    }

    [Fact]
    public async Task Student_Assignment_Is_Rejected_When_Tutor_Not_Approved()
    {
        await using var db = await CreateDbContextAsync();
        var studentService = new StudentOnboardingService(db);
        var tutorService = new TutorOnboardingService(db);

        var studentSave = await studentService.SaveOnboardingAsync(new StudentOnboardingModel
        {
            FirstName = "Approved",
            LastName = "Student",
            DateOfBirth = new DateTime(2011, 2, 2),
            EmergencyContactName = "Guardian",
            EmergencyContactPhone = "07333333333",
            RelationshipToStudent = "Parent",
            SENIndicators = "Communication support required",
            EHCPStatus = "In Place",
            ConsentDataSharing = true,
            ConsentEmailCommunication = true,
            PreferredContactMethod = "Email",
            PrivacyNoticeAcknowledged = true,
            DeclarationConfirmed = true
        });

        var tutorSave = await tutorService.SaveOnboardingAsync(new TutorOnboardingModel
        {
            FirstName = "Pending",
            LastName = "Tutor",
            DateOfBirth = new DateTime(1987, 5, 5),
            Email = "pending.tutor@example.com",
            Phone = "07444444444",
            HighestQualification = "BEd",
            TeachingExperience = "4 years tutoring",
            Reference1Name = "Ref Person",
            Reference1Contact = "ref@example.com",
            DBSStatus = "Completed",
            RightToWorkStatus = "UK Citizen",
            SafeguardingTrainingStatus = "Completed",
            ContractType = "Part-Time",
            ConsentDataProcessing = true,
            ConsentDBSCheck = true,
            ConsentReferences = true,
            PrivacyNoticeAcknowledged = true,
            DeclarationConfirmed = true
        });

        Assert.True(studentSave.Success);
        Assert.True(tutorSave.Success);
        Assert.NotNull(studentSave.Data);
        Assert.NotNull(tutorSave.Data);

        var studentApproved = await studentService.ApproveOnboardingAsync(studentSave.Data!.Id, "Approved student");
        Assert.True(studentApproved);

        var assigned = await studentService.AssignTutorAsync(studentSave.Data!.Id, tutorSave.Data!.Id, "Pending Tutor");
        var fromDb = await studentService.GetOnboardingByIdAsync(studentSave.Data.Id);

        Assert.False(assigned);
        Assert.NotNull(fromDb);
        Assert.Null(fromDb!.AssignedTutorId);
        Assert.Null(fromDb.AssignedTutorName);
    }

    [Fact]
    public async Task Carer_Onboarding_Persists_Consent_Updates_And_History()
    {
        await using var db = await CreateDbContextAsync();
        var service = new CarerOnboardingService(db);

        var save = await service.SaveOnboardingAsync(new CarerOnboardingData
        {
            FirstName = "Katherine",
            LastName = "Johnson",
            Email = "katherine@example.com",
            PhoneNumber = "07000000000",
            StudentId = "STU1001",
            StudentName = "Ada Lovelace",
            Relationship = "Guardian",
            StudentDateOfBirth = new DateTime(2012, 6, 1),
            EHCPStatus = "In Place",
            HasParentalResponsibility = true,
            NoRestrictiveOrders = true,
            PreferredContactMethod = "Email",
            ConfirmAccuracyAndTruth = true,
            ConsentsProvided = new ConsentStatus
            {
                PrivacyNoticeAcknowledged = true,
                DailyUpdatesConsent = true,
                PhotosVideosConsent = false,
                ThirdPartySharingConsent = false,
                LegitimateInterestConsent = true,
                TermsAccepted = true
            }
        });

        Assert.True(save.Success);
        Assert.NotNull(save.CarerId);
        var carerId = save.CarerId!;

        var approved = await service.ApproveOnboardingAsync(carerId, "Identity checked");
        var consentUpdated = await service.UpdateConsentAsync(carerId, new ConsentUpdate
        {
            CarerId = carerId,
            ConsentType = ConsentType.PhotosVideos,
            IsGranting = true,
            Reason = "Consent added by guardian"
        });

        var all = (await service.GetAllOnboardingsAsync()).ToList();
        var fromDb = await service.GetCarerByIdAsync(carerId);
        var history = await service.GetConsentHistoryAsync(carerId);

        Assert.True(approved);
        Assert.True(consentUpdated);
        Assert.Contains(all, x => x.CarerId == carerId);
        Assert.NotNull(fromDb);
        Assert.Equal(OnboardingStatus.Approved, fromDb!.Status);
        Assert.True(fromDb.ConsentsProvided.PhotosVideosConsent);
        Assert.True(history.Entries.Count >= 2);
        Assert.Contains(history.Entries, x => x.ConsentType == ConsentType.PhotosVideos && x.Granted);
    }

    [Fact]
    public async Task Student_Can_Be_Assigned_To_Multiple_Tutors_And_Each_Tutor_Sees_Student()
    {
        await using var db = await CreateDbContextAsync();
        var studentService = new StudentOnboardingService(db);
        var tutorOnboardingService = new TutorOnboardingService(db);
        var tutorPortalService = new TutorPortalService(db);

        var studentSave = await studentService.SaveOnboardingAsync(new StudentOnboardingModel
        {
            FirstName = "Multi",
            LastName = "Student",
            DateOfBirth = new DateTime(2011, 3, 3),
            EmergencyContactName = "Guardian",
            EmergencyContactPhone = "07555555555",
            RelationshipToStudent = "Parent",
            SENIndicators = "Numeracy support",
            EHCPStatus = "In Place",
            ConsentDataSharing = true,
            ConsentEmailCommunication = true,
            PreferredContactMethod = "Email",
            PrivacyNoticeAcknowledged = true,
            DeclarationConfirmed = true
        });

        var tutorOneSave = await tutorOnboardingService.SaveOnboardingAsync(new TutorOnboardingModel
        {
            FirstName = "Tutor",
            LastName = "One",
            DateOfBirth = new DateTime(1986, 4, 4),
            Email = "tutor.one@example.com",
            Phone = "07666666666",
            HighestQualification = "BEd",
            TeachingExperience = "SEN tutoring",
            Reference1Name = "Ref One",
            Reference1Contact = "ref1@example.com",
            DBSStatus = "Completed",
            RightToWorkStatus = "UK Citizen",
            SafeguardingTrainingStatus = "Completed",
            ContractType = "Part-Time",
            ConsentDataProcessing = true,
            ConsentDBSCheck = true,
            ConsentReferences = true,
            PrivacyNoticeAcknowledged = true,
            DeclarationConfirmed = true
        });

        var tutorTwoSave = await tutorOnboardingService.SaveOnboardingAsync(new TutorOnboardingModel
        {
            FirstName = "Tutor",
            LastName = "Two",
            DateOfBirth = new DateTime(1985, 5, 5),
            Email = "tutor.two@example.com",
            Phone = "07777777777",
            HighestQualification = "PGCE",
            TeachingExperience = "Literacy tutoring",
            Reference1Name = "Ref Two",
            Reference1Contact = "ref2@example.com",
            DBSStatus = "Completed",
            RightToWorkStatus = "UK Citizen",
            SafeguardingTrainingStatus = "Completed",
            ContractType = "Part-Time",
            ConsentDataProcessing = true,
            ConsentDBSCheck = true,
            ConsentReferences = true,
            PrivacyNoticeAcknowledged = true,
            DeclarationConfirmed = true
        });

        Assert.True(studentSave.Success);
        Assert.True(tutorOneSave.Success);
        Assert.True(tutorTwoSave.Success);
        Assert.NotNull(studentSave.Data);
        Assert.NotNull(tutorOneSave.Data);
        Assert.NotNull(tutorTwoSave.Data);

        var studentId = studentSave.Data!.Id;
        var tutorOneId = tutorOneSave.Data!.Id;
        var tutorTwoId = tutorTwoSave.Data!.Id;

        Assert.True(await studentService.ApproveOnboardingAsync(studentId, "Approved student"));
        Assert.True(await tutorOnboardingService.ApproveOnboardingAsync(tutorOneId, "Approved tutor one"));
        Assert.True(await tutorOnboardingService.ApproveOnboardingAsync(tutorTwoId, "Approved tutor two"));

        var assignedOne = await studentService.AssignTutorAsync(studentId, tutorOneId, "Tutor One");
        var assignedTwo = await studentService.AssignTutorAsync(studentId, tutorTwoId, "Tutor Two");
        var duplicateAssign = await studentService.AssignTutorAsync(studentId, tutorOneId, "Tutor One");

        Assert.True(assignedOne);
        Assert.True(assignedTwo);
        Assert.True(duplicateAssign);

        var assignmentCount = await db.StudentTutorAssignments
            .CountAsync(x => x.StudentOnboardingId == studentId);
        Assert.Equal(2, assignmentCount);

        var student = await studentService.GetOnboardingByIdAsync(studentId);
        Assert.NotNull(student);
        Assert.Contains("Tutor One", student!.AssignedTutorName ?? string.Empty);
        Assert.Contains("Tutor Two", student.AssignedTutorName ?? string.Empty);

        var tutorOneStudents = await tutorPortalService.GetStudentsUnderCareAsync(tutorOneId);
        var tutorTwoStudents = await tutorPortalService.GetStudentsUnderCareAsync(tutorTwoId);

        Assert.Contains(tutorOneStudents, s => s.StudentOnboardingId == studentId);
        Assert.Contains(tutorTwoStudents, s => s.StudentOnboardingId == studentId);
    }

    private static async Task<StudyMgtDbContext> CreateDbContextAsync()
    {
        var dbName = $"studymgt_integration_{Guid.NewGuid():N}";
        var connStr = $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=@Authenticate01";
        var options = new DbContextOptionsBuilder<StudyMgtDbContext>()
            .UseNpgsql(connStr)
            .Options;

        var db = new StudyMgtDbContext(options);
        await db.Database.MigrateAsync();
        return db;
    }
}
