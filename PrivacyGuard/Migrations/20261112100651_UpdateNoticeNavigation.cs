using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNoticeNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Notices_NoticeTemplateId",
                table: "Notices",
                column: "NoticeTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_NoticeTemplates_NoticeTemplateId",
                table: "Notices",
                column: "NoticeTemplateId",
                principalTable: "NoticeTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notices_NoticeTemplates_NoticeTemplateId",
                table: "Notices");

            migrationBuilder.DropIndex(
                name: "IX_Notices_NoticeTemplateId",
                table: "Notices");
        }
    }
}
