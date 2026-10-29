using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddedPurposeDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UnitCode",
                table: "OrgUnits",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ConsentPurposes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "DoesConsentLapse",
                table: "ConsentPurposes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RetentionPeriod",
                table: "ConsentPurposes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "ConsentPurposes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DataCategories",
                columns: table => new
                {
                    DataCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sensitivity = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataCategories", x => x.DataCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessors",
                columns: table => new
                {
                    DataProcessorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Processor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessors", x => x.DataProcessorId);
                });

            migrationBuilder.CreateTable(
                name: "ConsentPurposeDataCategories",
                columns: table => new
                {
                    ConsentPurposeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentPurposeDataCategories", x => new { x.ConsentPurposeId, x.DataCategoryId });
                    table.ForeignKey(
                        name: "FK_ConsentPurposeDataCategories_ConsentPurposes_ConsentPurposeId",
                        column: x => x.ConsentPurposeId,
                        principalTable: "ConsentPurposes",
                        principalColumn: "ConsentPurposeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsentPurposeDataCategories_DataCategories_DataCategoryId",
                        column: x => x.DataCategoryId,
                        principalTable: "DataCategories",
                        principalColumn: "DataCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsentPurposeDataProcessors",
                columns: table => new
                {
                    ConsentPurposeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataProcessorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentPurposeDataProcessors", x => new { x.ConsentPurposeId, x.DataProcessorId });
                    table.ForeignKey(
                        name: "FK_ConsentPurposeDataProcessors_ConsentPurposes_ConsentPurposeId",
                        column: x => x.ConsentPurposeId,
                        principalTable: "ConsentPurposes",
                        principalColumn: "ConsentPurposeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsentPurposeDataProcessors_DataProcessors_DataProcessorId",
                        column: x => x.DataProcessorId,
                        principalTable: "DataProcessors",
                        principalColumn: "DataProcessorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnits_OrganizationId_UnitCode",
                table: "OrgUnits",
                columns: new[] { "OrganizationId", "UnitCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsentPurposeDataCategories_DataCategoryId",
                table: "ConsentPurposeDataCategories",
                column: "DataCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentPurposeDataProcessors_DataProcessorId",
                table: "ConsentPurposeDataProcessors",
                column: "DataProcessorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsentPurposeDataCategories");

            migrationBuilder.DropTable(
                name: "ConsentPurposeDataProcessors");

            migrationBuilder.DropTable(
                name: "DataCategories");

            migrationBuilder.DropTable(
                name: "DataProcessors");

            migrationBuilder.DropIndex(
                name: "IX_OrgUnits_OrganizationId_UnitCode",
                table: "OrgUnits");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ConsentPurposes");

            migrationBuilder.DropColumn(
                name: "DoesConsentLapse",
                table: "ConsentPurposes");

            migrationBuilder.DropColumn(
                name: "RetentionPeriod",
                table: "ConsentPurposes");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "ConsentPurposes");

            migrationBuilder.AlterColumn<string>(
                name: "UnitCode",
                table: "OrgUnits",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
