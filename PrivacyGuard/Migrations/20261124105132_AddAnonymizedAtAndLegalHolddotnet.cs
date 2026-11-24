using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddAnonymizedAtAndLegalHolddotnet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrincipalIdentifier_DataPrincipals_DataPrincipalId",
                table: "PrincipalIdentifier");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PrincipalIdentifier",
                table: "PrincipalIdentifier");

            migrationBuilder.RenameTable(
                name: "PrincipalIdentifier",
                newName: "PrincipalIdentifiers");

            migrationBuilder.RenameIndex(
                name: "IX_PrincipalIdentifier_DataPrincipalId",
                table: "PrincipalIdentifiers",
                newName: "IX_PrincipalIdentifiers_DataPrincipalId");

            migrationBuilder.AddColumn<DateTime>(
                name: "AnonymizedAt",
                table: "DataPrincipals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnLegalHold",
                table: "DataPrincipals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "DataPrincipals",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PrincipalIdentifiers",
                table: "PrincipalIdentifiers",
                column: "IdentifierId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrincipalIdentifiers_DataPrincipals_DataPrincipalId",
                table: "PrincipalIdentifiers",
                column: "DataPrincipalId",
                principalTable: "DataPrincipals",
                principalColumn: "DataPrincipalId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrincipalIdentifiers_DataPrincipals_DataPrincipalId",
                table: "PrincipalIdentifiers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PrincipalIdentifiers",
                table: "PrincipalIdentifiers");

            migrationBuilder.DropColumn(
                name: "AnonymizedAt",
                table: "DataPrincipals");

            migrationBuilder.DropColumn(
                name: "IsOnLegalHold",
                table: "DataPrincipals");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "DataPrincipals");

            migrationBuilder.RenameTable(
                name: "PrincipalIdentifiers",
                newName: "PrincipalIdentifier");

            migrationBuilder.RenameIndex(
                name: "IX_PrincipalIdentifiers_DataPrincipalId",
                table: "PrincipalIdentifier",
                newName: "IX_PrincipalIdentifier_DataPrincipalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PrincipalIdentifier",
                table: "PrincipalIdentifier",
                column: "IdentifierId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrincipalIdentifier_DataPrincipals_DataPrincipalId",
                table: "PrincipalIdentifier",
                column: "DataPrincipalId",
                principalTable: "DataPrincipals",
                principalColumn: "DataPrincipalId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
