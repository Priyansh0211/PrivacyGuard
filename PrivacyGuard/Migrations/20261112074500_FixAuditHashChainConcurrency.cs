using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    public partial class FixAuditHashChainConcurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OrganizationId_SequenceNumber",
                table: "AuditLogs",
                columns: new[] { "OrganizationId", "SequenceNumber" },
                unique: true,
                filter: "[OrganizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SequenceNumber_System",
                table: "AuditLogs",
                column: "SequenceNumber",
                unique: true,
                filter: "[OrganizationId] IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_OrganizationId_SequenceNumber",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_SequenceNumber_System",
                table: "AuditLogs");
        }
    }
}
