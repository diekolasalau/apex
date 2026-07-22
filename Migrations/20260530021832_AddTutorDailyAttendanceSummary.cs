using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyMgt.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorDailyAttendanceSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TutorDailyAttendanceSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TutorOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AttendanceDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalMinutes = table.Column<int>(type: "integer", nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorDailyAttendanceSummaries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorDailyAttendanceSummaries_AttendanceDateUtc",
                table: "TutorDailyAttendanceSummaries",
                column: "AttendanceDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TutorDailyAttendanceSummaries_TutorOnboardingId",
                table: "TutorDailyAttendanceSummaries",
                column: "TutorOnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorDailyAttendanceSummaries_TutorOnboardingId_Username_At~",
                table: "TutorDailyAttendanceSummaries",
                columns: new[] { "TutorOnboardingId", "Username", "AttendanceDateUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorDailyAttendanceSummaries_Username",
                table: "TutorDailyAttendanceSummaries",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TutorDailyAttendanceSummaries");
        }
    }
}
