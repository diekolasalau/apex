using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyMgt.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentMultiTutorAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentTutorAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    TutorOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    TutorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AssignedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTutorAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTutorAssignments_StudentOnboardings_StudentOnboardin~",
                        column: x => x.StudentOnboardingId,
                        principalTable: "StudentOnboardings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentTutorAssignments_StudentOnboardingId",
                table: "StudentTutorAssignments",
                column: "StudentOnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTutorAssignments_StudentOnboardingId_TutorOnboarding~",
                table: "StudentTutorAssignments",
                columns: new[] { "StudentOnboardingId", "TutorOnboardingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentTutorAssignments_TutorOnboardingId",
                table: "StudentTutorAssignments",
                column: "TutorOnboardingId");

            migrationBuilder.Sql(@"
                INSERT INTO ""StudentTutorAssignments"" (""StudentOnboardingId"", ""TutorOnboardingId"", ""TutorName"", ""AssignedDateUtc"")
                SELECT
                    s.""Id"",
                    s.""AssignedTutorId"",
                    COALESCE(NULLIF(s.""AssignedTutorName"", ''), 'Tutor #' || s.""AssignedTutorId""::text),
                    COALESCE(s.""AssignedDate"", NOW())
                FROM ""StudentOnboardings"" s
                WHERE s.""AssignedTutorId"" IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentTutorAssignments");
        }
    }
}
