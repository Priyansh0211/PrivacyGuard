using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    public partial class AddRoleToDataPrincipals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "DataPrincipals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataPrincipals_RoleId",
                table: "DataPrincipals",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataPrincipals_Roles_RoleId",
                table: "DataPrincipals",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPrincipals_Roles_RoleId",
                table: "DataPrincipals");

            migrationBuilder.DropIndex(
                name: "IX_DataPrincipals_RoleId",
                table: "DataPrincipals");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "DataPrincipals");
        }
    }
}
