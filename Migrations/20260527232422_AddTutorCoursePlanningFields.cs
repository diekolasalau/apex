using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyMgt.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorCoursePlanningFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseDuration",
                table: "TutorOnboardings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoursesToBeTaken",
                table: "TutorOnboardings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseDuration",
                table: "TutorOnboardings");

            migrationBuilder.DropColumn(
                name: "CoursesToBeTaken",
                table: "TutorOnboardings");
        }
    }
}
