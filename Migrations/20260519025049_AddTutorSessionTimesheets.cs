using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyMgt.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorSessionTimesheets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TutorMonthlyTimesheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TutorOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    TutorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    TotalMinutes = table.Column<int>(type: "integer", nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorMonthlyTimesheets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TutorPortalSessionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TutorPortalAccountId = table.Column<int>(type: "integer", nullable: false),
                    TutorOnboardingId = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LoginAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LogoutAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorPortalSessionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorPortalSessionLogs_TutorPortalAccounts_TutorPortalAccou~",
                        column: x => x.TutorPortalAccountId,
                        principalTable: "TutorPortalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorMonthlyTimesheets_TutorOnboardingId_Year_Month",
                table: "TutorMonthlyTimesheets",
                columns: new[] { "TutorOnboardingId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorMonthlyTimesheets_Username",
                table: "TutorMonthlyTimesheets",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_TutorPortalSessionLogs_IsClosed",
                table: "TutorPortalSessionLogs",
                column: "IsClosed");

            migrationBuilder.CreateIndex(
                name: "IX_TutorPortalSessionLogs_LoginAtUtc",
                table: "TutorPortalSessionLogs",
                column: "LoginAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TutorPortalSessionLogs_TutorOnboardingId",
                table: "TutorPortalSessionLogs",
                column: "TutorOnboardingId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorPortalSessionLogs_TutorPortalAccountId",
                table: "TutorPortalSessionLogs",
                column: "TutorPortalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorPortalSessionLogs_Username",
                table: "TutorPortalSessionLogs",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TutorMonthlyTimesheets");

            migrationBuilder.DropTable(
                name: "TutorPortalSessionLogs");
        }
    }
}
