using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddDataBreachModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataBreachIncidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscoveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedAffectedRecords = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MitigationSteps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataBreachIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataBreachIncidents_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataBreachIncidents_Users_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataBreachIncidents_AssignedToUserId",
                table: "DataBreachIncidents",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataBreachIncidents_ReportedByUserId",
                table: "DataBreachIncidents",
                column: "ReportedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataBreachIncidents");
        }
    }
}
