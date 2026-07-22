using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyMgt.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarerOnboardings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarerId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    StudentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StudentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Relationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StudentDateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EHCPStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HasParentalResponsibility = table.Column<bool>(type: "boolean", nullable: false),
                    NoRestrictiveOrders = table.Column<bool>(type: "boolean", nullable: false),
                    PreferredContactMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MedicalAndAccessibilityInfo = table.Column<string>(type: "text", nullable: true),
                    MedicalAndAccessibilityInfo2 = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmergencyContactRelationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrivacyNoticeAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    PrivacyNoticeAcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DailyUpdatesConsent = table.Column<bool>(type: "boolean", nullable: false),
                    DailyUpdatesConsentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DailyUpdatesWithdrawn = table.Column<bool>(type: "boolean", nullable: true),
                    PhotosVideosConsent = table.Column<bool>(type: "boolean", nullable: false),
                    PhotosVideosConsentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhotosVideosWithdrawn = table.Column<bool>(type: "boolean", nullable: true),
                    ThirdPartySharingConsent = table.Column<bool>(type: "boolean", nullable: false),
                    ThirdPartySharingConsentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ThirdPartySharingWithdrawn = table.Column<bool>(type: "boolean", nullable: true),
                    LegitimateInterestConsent = table.Column<bool>(type: "boolean", nullable: false),
                    LegitimateInterestConsentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TermsAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    TermsAcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmAccuracyAndTruth = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovalNotes = table.Column<string>(type: "text", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataRetentionExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataRetentionCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarerOnboardings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbRoundTripLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbRoundTripLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentOnboardings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StudentIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmergencyContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmergencyContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RelationshipToStudent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SENIndicators = table.Column<string>(type: "text", nullable: false),
                    EHCPStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EHCPDocumentPath = table.Column<string>(type: "text", nullable: true),
                    ILPSummary = table.Column<string>(type: "text", nullable: true),
                    RequiresPhysicalAccommodation = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresHearingSupport = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresVisualSupport = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresCommunicationSupport = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresBehaviorSupport = table.Column<bool>(type: "boolean", nullable: false),
                    SafeguardingNotes = table.Column<string>(type: "text", nullable: true),
                    MedicalInformation = table.Column<string>(type: "text", nullable: true),
                    RiskAssessmentNotes = table.Column<string>(type: "text", nullable: true),
                    ConsentDataSharing = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentPhotos = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentEmailCommunication = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentSMSCommunication = table.Column<bool>(type: "boolean", nullable: false),
                    PreferredContactMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrivacyNoticeAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    DeclarationConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedTutorId = table.Column<int>(type: "integer", nullable: true),
                    AssignedTutorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentOnboardings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TutorOnboardings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    HighestQualification = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OtherQualifications = table.Column<string>(type: "text", nullable: true),
                    TeachingExperience = table.Column<string>(type: "text", nullable: false),
                    Reference1Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Reference1Contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Reference2Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Reference2Contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DBSStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DBSCertificateNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DBSIssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DBSExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DBSCheckType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RightToWorkStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VisaType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VisaExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PassportNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SafeguardingTrainingStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SafeguardingTrainingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SafeguardingTrainingExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrainingProvider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContractType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContractStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContractTerms = table.Column<string>(type: "text", nullable: true),
                    ConsentDataProcessing = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentDBSCheck = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentReferences = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentMarketing = table.Column<bool>(type: "boolean", nullable: false),
                    PrivacyNoticeAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    DeclarationConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorOnboardings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CarerConsentAuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarerOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Granted = table.Column<bool>(type: "boolean", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarerConsentAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarerConsentAuditEntries_CarerOnboardings_CarerOnboardingId",
                        column: x => x.CarerOnboardingId,
                        principalTable: "CarerOnboardings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentConsentRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    ConsentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsConsented = table.Column<bool>(type: "boolean", nullable: false),
                    RecordedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentConsentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentConsentRecords_StudentOnboardings_StudentOnboardingId",
                        column: x => x.StudentOnboardingId,
                        principalTable: "StudentOnboardings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentOnboardingAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FieldChanged = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentOnboardingAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentOnboardingAudits_StudentOnboardings_StudentOnboardin~",
                        column: x => x.StudentOnboardingId,
                        principalTable: "StudentOnboardings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorOnboardingAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TutorOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FieldChanged = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorOnboardingAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorOnboardingAudits_TutorOnboardings_TutorOnboardingId",
                        column: x => x.TutorOnboardingId,
                        principalTable: "TutorOnboardings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarerConsentAuditEntries_CarerOnboardingId",
                table: "CarerConsentAuditEntries",
                column: "CarerOnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_CarerConsentAuditEntries_Timestamp",
                table: "CarerConsentAuditEntries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_CarerOnboardings_CarerId",
                table: "CarerOnboardings",
                column: "CarerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarerOnboardings_CreatedAt",
                table: "CarerOnboardings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CarerOnboardings_Status",
                table: "CarerOnboardings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StudentConsentRecords_RecordedDate",
                table: "StudentConsentRecords",
                column: "RecordedDate");

            migrationBuilder.CreateIndex(
                name: "IX_StudentConsentRecords_StudentOnboardingId",
                table: "StudentConsentRecords",
                column: "StudentOnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentOnboardingAudits_ChangedDate",
                table: "StudentOnboardingAudits",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_StudentOnboardingAudits_StudentOnboardingId",
                table: "StudentOnboardingAudits",
                column: "StudentOnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentOnboardings_Status",
                table: "StudentOnboardings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StudentOnboardings_SubmittedDate",
                table: "StudentOnboardings",
                column: "SubmittedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TutorOnboardingAudits_ChangedDate",
                table: "TutorOnboardingAudits",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TutorOnboardingAudits_TutorOnboardingId",
                table: "TutorOnboardingAudits",
                column: "TutorOnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorOnboardings_Email",
                table: "TutorOnboardings",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_TutorOnboardings_Status",
                table: "TutorOnboardings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TutorOnboardings_SubmittedDate",
                table: "TutorOnboardings",
                column: "SubmittedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarerConsentAuditEntries");

            migrationBuilder.DropTable(
                name: "DbRoundTripLogs");

            migrationBuilder.DropTable(
                name: "StudentConsentRecords");

            migrationBuilder.DropTable(
                name: "StudentOnboardingAudits");

            migrationBuilder.DropTable(
                name: "TutorOnboardingAudits");

            migrationBuilder.DropTable(
                name: "CarerOnboardings");

            migrationBuilder.DropTable(
                name: "StudentOnboardings");

            migrationBuilder.DropTable(
                name: "TutorOnboardings");
        }
    }
}
