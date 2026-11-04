using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    /// <inheritdoc />
    public partial class DataPrincipal360Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoticeTranslations_Notices_NoticeId",
                table: "NoticeTranslations");

            migrationBuilder.AlterColumn<Guid>(
                name: "DataPrincipalId",
                table: "PrivacyRequests",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "PrincipalIdentifier",
                columns: table => new
                {
                    IdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataPrincipalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrincipalIdentifier", x => x.IdentifierId);
                    table.ForeignKey(
                        name: "FK_PrincipalIdentifier_DataPrincipals_DataPrincipalId",
                        column: x => x.DataPrincipalId,
                        principalTable: "DataPrincipals",
                        principalColumn: "DataPrincipalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyRequests_DataPrincipalId",
                table: "PrivacyRequests",
                column: "DataPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_DataPrincipalId",
                table: "ConsentRecords",
                column: "DataPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_PrincipalIdentifier_DataPrincipalId",
                table: "PrincipalIdentifier",
                column: "DataPrincipalId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentRecords_DataPrincipals_DataPrincipalId",
                table: "ConsentRecords",
                column: "DataPrincipalId",
                principalTable: "DataPrincipals",
                principalColumn: "DataPrincipalId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeTranslations_Notices_NoticeId",
                table: "NoticeTranslations",
                column: "NoticeId",
                principalTable: "Notices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivacyRequests_DataPrincipals_DataPrincipalId",
                table: "PrivacyRequests",
                column: "DataPrincipalId",
                principalTable: "DataPrincipals",
                principalColumn: "DataPrincipalId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsentRecords_DataPrincipals_DataPrincipalId",
                table: "ConsentRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_NoticeTranslations_Notices_NoticeId",
                table: "NoticeTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivacyRequests_DataPrincipals_DataPrincipalId",
                table: "PrivacyRequests");

            migrationBuilder.DropTable(
                name: "PrincipalIdentifier");

            migrationBuilder.DropIndex(
                name: "IX_PrivacyRequests_DataPrincipalId",
                table: "PrivacyRequests");

            migrationBuilder.DropIndex(
                name: "IX_ConsentRecords_DataPrincipalId",
                table: "ConsentRecords");

            migrationBuilder.AlterColumn<string>(
                name: "DataPrincipalId",
                table: "PrivacyRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_NoticeTranslations_Notices_NoticeId",
                table: "NoticeTranslations",
                column: "NoticeId",
                principalTable: "Notices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
