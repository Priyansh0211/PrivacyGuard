using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivacyGuard.Migrations
{
    /// <inheritdoc />
    public partial class dataprincipleGrivanceedit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grievances_Users_DataPrincipalId",
                table: "Grievances");

            // Existing rows were incorrectly stored with Users.UserId.
            // Convert them to DataPrincipals.DataPrincipalId before adding
            // the new foreign key.
            migrationBuilder.Sql(@"
                DECLARE @FallbackDataPrincipalId uniqueidentifier = '4e893124-4705-488d-9820-83c3825fd24c';

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM DataPrincipals
                    WHERE DataPrincipalId = @FallbackDataPrincipalId
                )
                BEGIN
                    THROW 50002,
                        'Fallback DataPrincipalId 4e893124-4705-488d-9820-83c3825fd24c does not exist.',
                        1;
                END;

                UPDATE g
                SET g.DataPrincipalId = COALESCE(
                    dpByReference.DataPrincipalId,
                    dpByEmail.DataPrincipalId,
                    @FallbackDataPrincipalId)
                FROM Grievances AS g
                INNER JOIN Users AS u
                    ON u.UserId = g.DataPrincipalId
                LEFT JOIN DataPrincipals AS dpByReference
                    ON dpByReference.ReferenceId = u.ReferenceId
                LEFT JOIN DataPrincipals AS dpByEmail
                    ON dpByReference.DataPrincipalId IS NULL
                   AND dpByEmail.Email IS NOT NULL
                   AND dpByEmail.Email = u.Email;

            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Grievances_DataPrincipals_DataPrincipalId",
                table: "Grievances",
                column: "DataPrincipalId",
                principalTable: "DataPrincipals",
                principalColumn: "DataPrincipalId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grievances_DataPrincipals_DataPrincipalId",
                table: "Grievances");

            migrationBuilder.AddForeignKey(
                name: "FK_Grievances_Users_DataPrincipalId",
                table: "Grievances",
                column: "DataPrincipalId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
