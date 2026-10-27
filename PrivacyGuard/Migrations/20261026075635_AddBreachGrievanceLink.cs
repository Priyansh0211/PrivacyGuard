using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddBreachGrievanceLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceGrievanceId",
                table: "DataBreachIncidents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataBreachIncidents_SourceGrievanceId",
                table: "DataBreachIncidents",
                column: "SourceGrievanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataBreachIncidents_Grievances_SourceGrievanceId",
                table: "DataBreachIncidents",
                column: "SourceGrievanceId",
                principalTable: "Grievances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataBreachIncidents_Grievances_SourceGrievanceId",
                table: "DataBreachIncidents");

            migrationBuilder.DropIndex(
                name: "IX_DataBreachIncidents_SourceGrievanceId",
                table: "DataBreachIncidents");

            migrationBuilder.DropColumn(
                name: "SourceGrievanceId",
                table: "DataBreachIncidents");
        }
    }
}
