using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyMgt.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PagePath = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ActorRole = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ActorUsername = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    EntityType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    EntityId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Details = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAuditLogs_Action",
                table: "ApplicationAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAuditLogs_EventType",
                table: "ApplicationAuditLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAuditLogs_OccurredAtUtc",
                table: "ApplicationAuditLogs",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAuditLogs_PagePath",
                table: "ApplicationAuditLogs",
                column: "PagePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationAuditLogs");
        }
    }
}
