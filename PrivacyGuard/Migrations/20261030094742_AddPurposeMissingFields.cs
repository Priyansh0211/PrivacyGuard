using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddPurposeMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "DataProcessors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "DataLeavesIndia",
                table: "DataProcessors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                table: "DataProcessors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DataCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DataCategories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ConsentPurposes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsProhibitedForChildren",
                table: "ConsentPurposes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "DataProcessors");

            migrationBuilder.DropColumn(
                name: "DataLeavesIndia",
                table: "DataProcessors");

            migrationBuilder.DropColumn(
                name: "LegalName",
                table: "DataProcessors");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DataCategories");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "DataCategories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ConsentPurposes");

            migrationBuilder.DropColumn(
                name: "IsProhibitedForChildren",
                table: "ConsentPurposes");
        }
    }
}
