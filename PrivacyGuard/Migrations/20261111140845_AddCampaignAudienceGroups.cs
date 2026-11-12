using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignAudienceGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignAudienceGroups",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignAudienceGroups", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "CampaignAudienceGroupMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignAudienceGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignAudienceGroupMembers_CampaignAudienceGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "CampaignAudienceGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAudienceGroupMembers_GroupId_MemberId_UserType",
                table: "CampaignAudienceGroupMembers",
                columns: new[] { "GroupId", "MemberId", "UserType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignAudienceGroups_OrganizationId_Name",
                table: "CampaignAudienceGroups",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignAudienceGroupMembers");

            migrationBuilder.DropTable(
                name: "CampaignAudienceGroups");
        }
    }
}
