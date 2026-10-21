using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddConsentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsentRecords_Users_UserId",
                table: "ConsentRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConsentRecords",
                table: "ConsentRecords");

            migrationBuilder.DropIndex(
                name: "IX_ConsentRecords_UserId",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "ConsentPurpose",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "DeviceUserAgent",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "IsGranted",
                table: "ConsentRecords");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ConsentRecords",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "ConsentId",
                table: "ConsentRecords",
                newName: "DataInventoryId");

            migrationBuilder.AlterColumn<string>(
                name: "NoticeVersion",
                table: "ConsentRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<Guid>(
                name: "ConsentRecordId",
                table: "ConsentRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ConsentStatus",
                table: "ConsentRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataPrincipalId",
                table: "ConsentRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConsentRecords",
                table: "ConsentRecords",
                column: "ConsentRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_DataInventoryId",
                table: "ConsentRecords",
                column: "DataInventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentRecords_DataInventories_DataInventoryId",
                table: "ConsentRecords",
                column: "DataInventoryId",
                principalTable: "DataInventories",
                principalColumn: "DataInventoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsentRecords_DataInventories_DataInventoryId",
                table: "ConsentRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConsentRecords",
                table: "ConsentRecords");

            migrationBuilder.DropIndex(
                name: "IX_ConsentRecords_DataInventoryId",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "ConsentRecordId",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "ConsentStatus",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "DataPrincipalId",
                table: "ConsentRecords");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "ConsentRecords",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "DataInventoryId",
                table: "ConsentRecords",
                newName: "ConsentId");

            migrationBuilder.AlterColumn<string>(
                name: "NoticeVersion",
                table: "ConsentRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "ConsentPurpose",
                table: "ConsentRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeviceUserAgent",
                table: "ConsentRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "ConsentRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "ConsentRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGranted",
                table: "ConsentRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConsentRecords",
                table: "ConsentRecords",
                column: "ConsentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_UserId",
                table: "ConsentRecords",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentRecords_Users_UserId",
                table: "ConsentRecords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
