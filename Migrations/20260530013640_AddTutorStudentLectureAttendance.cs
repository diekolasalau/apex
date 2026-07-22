using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyMgt.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorStudentLectureAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TutorStudentLectureAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TutorPortalAccountId = table.Column<int>(type: "integer", nullable: false),
                    TutorOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    StudentOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LectureStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LectureEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorStudentLectureAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorStudentLectureAttendances_TutorPortalAccounts_TutorPor~",
                        column: x => x.TutorPortalAccountId,
                        principalTable: "TutorPortalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorStudentLectureAttendances_IsClosed",
                table: "TutorStudentLectureAttendances",
                column: "IsClosed");

            migrationBuilder.CreateIndex(
                name: "IX_TutorStudentLectureAttendances_LectureStartUtc",
                table: "TutorStudentLectureAttendances",
                column: "LectureStartUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TutorStudentLectureAttendances_StudentOnboardingId",
                table: "TutorStudentLectureAttendances",
                column: "StudentOnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorStudentLectureAttendances_TutorOnboardingId",
                table: "TutorStudentLectureAttendances",
                column: "TutorOnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorStudentLectureAttendances_TutorPortalAccountId",
                table: "TutorStudentLectureAttendances",
                column: "TutorPortalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorStudentLectureAttendances_Username",
                table: "TutorStudentLectureAttendances",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TutorStudentLectureAttendances");
        }
    }
}
