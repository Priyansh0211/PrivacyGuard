using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    public partial class AddNoticeTranslationMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TranslationSource",
                table: "NoticeTranslations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Manual");

            migrationBuilder.AddColumn<string>(
                name: "TranslationStatus",
                table: "NoticeTranslations",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Approved");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TranslationSource",
                table: "NoticeTranslations");

            migrationBuilder.DropColumn(
                name: "TranslationStatus",
                table: "NoticeTranslations");
        }
    }
}
