using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StudyMgt.Data;
using StudyMgt.Data.Entities;

namespace StudyMgt.Services;

public static class DemoDataSeeder
{
    private const string SampleTutorFirstName = "Ishola";
    private const string SampleTutorLastName = "Ogunsola";
    private const string SampleTutorEmail = "ishola.ogunsola@sample.local";
    private const string SampleTutorUsername = "ishola.ogunsola";
    private const string SampleTutorPassword = "Ishola@1234!";
    private const string SampleAdminUsername = "admin";
    private const string SampleAdminPassword = "Admin1234";
    private const string SampleAdminEmail = "centre.admin@sample.local";
    private const string SampleAdminFullName = "Centre Administrator";
    private const string SampleCouncilUsername = "council.rep";
    private const string SampleCouncilPassword = "Council@123";
    private const string SampleCouncilEmail = "council.rep@sample.local";
    private const string SampleCouncilDisplayName = "Council Representative";

    public static async Task SeedSystemAccountsAsync(StudyMgtDbContext dbContext)
    {
        var admin = await dbContext.CentreAdminPortalAccounts
            .FirstOrDefaultAsync(x => x.Username == SampleAdminUsername);

        CreatePasswordHash(SampleAdminPassword, 10_000, out var adminPasswordHash, out var adminPasswordSalt);

        if (admin == null)
        {
            dbContext.CentreAdminPortalAccounts.Add(new CentreAdminPortalAccountEntity
            {
                Username = SampleAdminUsername,
                PasswordHash = adminPasswordHash,
                PasswordSalt = adminPasswordSalt,
                Email = SampleAdminEmail,
                FullName = SampleAdminFullName,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }
        else
        {
            admin.PasswordHash = adminPasswordHash;
            admin.PasswordSalt = adminPasswordSalt;
            admin.Email = SampleAdminEmail;
            admin.FullName = SampleAdminFullName;
            admin.IsActive = true;

            await dbContext.SaveChangesAsync();
        }

        var council = await dbContext.CouncilPortalAccounts
            .FirstOrDefaultAsync(x => x.Username == SampleCouncilUsername);

        CreatePasswordHash(SampleCouncilPassword, 10_000, out var councilPasswordHash, out var councilPasswordSalt);

        if (council == null)
        {
            dbContext.CouncilPortalAccounts.Add(new CouncilPortalAccountEntity
            {
                Username = SampleCouncilUsername,
                PasswordHash = councilPasswordHash,
                PasswordSalt = councilPasswordSalt,
                Email = SampleCouncilEmail,
                DisplayName = SampleCouncilDisplayName,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }
        else
        {
            council.PasswordHash = councilPasswordHash;
            council.PasswordSalt = councilPasswordSalt;
            council.Email = SampleCouncilEmail;
            council.DisplayName = SampleCouncilDisplayName;
            council.IsActive = true;
            council.FailedLoginAttempts = 0;
            council.LockedUntilUtc = null;

            await dbContext.SaveChangesAsync();
        }
    }

    public static async Task SeedTutorTimesheetSampleAsync(StudyMgtDbContext dbContext)
    {
        var tutor = await dbContext.TutorOnboardings
            .FirstOrDefaultAsync(x =>
                x.FirstName.ToLower() == SampleTutorFirstName.ToLower() &&
                x.LastName.ToLower() == SampleTutorLastName.ToLower());

        if (tutor == null)
        {
            tutor = new TutorOnboardingEntity
            {
                FirstName = SampleTutorFirstName,
                LastName = SampleTutorLastName,
                DateOfBirth = new DateTime(1988, 4, 17, 0, 0, 0, DateTimeKind.Utc),
                Gender = "Male",
                Email = SampleTutorEmail,
                Phone = "07123 456789",
                Address = "12 Demo Street, London",
                HighestQualification = "BSc Mathematics",
                TeachingExperience = "8 years",
                Reference1Name = "Demo Reference One",
                Reference1Contact = "ref1@example.com",
                Reference2Name = "Demo Reference Two",
                Reference2Contact = "ref2@example.com",
                DBSStatus = "Clear",
                DBSCertificateNumber = "DBS-SAMPLE-2026",
                DBSIssueDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                DBSExpiryDate = new DateTime(2029, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                DBSCheckType = "Enhanced",
                RightToWorkStatus = "Verified",
                SafeguardingTrainingStatus = "Current",
                SafeguardingTrainingDate = new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc),
                SafeguardingTrainingExpiry = new DateTime(2027, 3, 5, 0, 0, 0, DateTimeKind.Utc),
                TrainingProvider = "Safeguarding Academy",
                ContractType = "Monthly",
                ContractStartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ContractTerms = "Sample tutor contract used for portal demonstration.",
                ConsentDataProcessing = true,
                ConsentDBSCheck = true,
                ConsentReferences = true,
                ConsentMarketing = false,
                PrivacyNoticeAcknowledged = true,
                DeclarationConfirmed = true,
                Status = OnboardingStatus.Approved.ToString(),
                AdminNotes = "Seeded sample tutor record for monthly timesheet demo.",
                SubmittedDate = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc),
                ApprovedDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                ApprovedBy = "System"
            };

            dbContext.TutorOnboardings.Add(tutor);
            await dbContext.SaveChangesAsync();
        }

        var account = await dbContext.TutorPortalAccounts
            .FirstOrDefaultAsync(x => x.TutorOnboardingId == tutor.Id);

        if (account == null)
        {
            CreatePasswordHash(SampleTutorPassword, 100_000, out var passwordHash, out var passwordSalt);

            account = new TutorPortalAccountEntity
            {
                TutorOnboardingId = tutor.Id,
                Username = SampleTutorUsername,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.TutorPortalAccounts.Add(account);
            await dbContext.SaveChangesAsync();
        }

        var sampleMonth = DateTime.UtcNow.AddMonths(-1);
        var timesheet = await dbContext.TutorMonthlyTimesheets
            .FirstOrDefaultAsync(x =>
                x.TutorOnboardingId == tutor.Id &&
                x.Year == sampleMonth.Year &&
                x.Month == sampleMonth.Month);

        if (timesheet == null)
        {
            dbContext.TutorMonthlyTimesheets.Add(new TutorMonthlyTimesheetEntity
            {
                TutorOnboardingId = tutor.Id,
                TutorName = $"{tutor.FirstName} {tutor.LastName}".Trim(),
                Username = account.Username,
                Year = sampleMonth.Year,
                Month = sampleMonth.Month,
                SessionCount = 12,
                TotalMinutes = 720,
                GeneratedAtUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }
    }

    private static void CreatePasswordHash(string password, int iterations, out string hash, out string salt)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            iterations,
            HashAlgorithmName.SHA256,
            32);

        hash = Convert.ToBase64String(hashBytes);
        salt = Convert.ToBase64String(saltBytes);
    }
}