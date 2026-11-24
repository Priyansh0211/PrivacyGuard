using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PrivacyGuard.Data;

#nullable disable

namespace PrivacyGuard.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261118115000_AddDataPrincipalGrievanceMessageSender")]
    public partial class AddDataPrincipalGrievanceMessageSender : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SenderId",
                table: "GrievanceMessages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "SenderDataPrincipalId",
                table: "GrievanceMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrievanceMessages_SenderDataPrincipalId",
                table: "GrievanceMessages",
                column: "SenderDataPrincipalId");

            migrationBuilder.AddForeignKey(
                name: "FK_GrievanceMessages_DataPrincipals_SenderDataPrincipalId",
                table: "GrievanceMessages",
                column: "SenderDataPrincipalId",
                principalTable: "DataPrincipals",
                principalColumn: "DataPrincipalId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddCheckConstraint(
                name: "CK_GrievanceMessages_ExactlyOneSender",
                table: "GrievanceMessages",
                sql: "([SenderId] IS NOT NULL AND [SenderDataPrincipalId] IS NULL) OR ([SenderId] IS NULL AND [SenderDataPrincipalId] IS NOT NULL)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF EXISTS (SELECT 1 FROM [GrievanceMessages] WHERE [SenderDataPrincipalId] IS NOT NULL) " +
                "THROW 51000, 'Cannot revert while grievance messages have DataPrincipal senders.', 1;");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GrievanceMessages_ExactlyOneSender",
                table: "GrievanceMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GrievanceMessages_DataPrincipals_SenderDataPrincipalId",
                table: "GrievanceMessages");

            migrationBuilder.DropIndex(
                name: "IX_GrievanceMessages_SenderDataPrincipalId",
                table: "GrievanceMessages");

            migrationBuilder.DropColumn(
                name: "SenderDataPrincipalId",
                table: "GrievanceMessages");

            migrationBuilder.AlterColumn<Guid>(
                name: "SenderId",
                table: "GrievanceMessages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
