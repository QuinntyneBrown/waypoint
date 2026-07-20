using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordUp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforcePrivacyOwnershipIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_ApplicantUserId",
                table: "Applications",
                column: "ApplicantUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_CreatedYouthUserId",
                table: "Applications",
                column: "CreatedYouthUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Users_GuardianUserId",
                table: "Applications",
                column: "GuardianUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRequests_Users_OwnerUserId",
                table: "ContactRequests",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DonationPledges_Users_OwnerUserId",
                table: "DonationPledges",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OutboundEmails_Users_RecipientUserId",
                table: "OutboundEmails",
                column: "RecipientUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerSignups_Users_OwnerUserId",
                table: "VolunteerSignups",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_ApplicantUserId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_CreatedYouthUserId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_GuardianUserId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactRequests_Users_OwnerUserId",
                table: "ContactRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationPledges_Users_OwnerUserId",
                table: "DonationPledges");

            migrationBuilder.DropForeignKey(
                name: "FK_OutboundEmails_Users_RecipientUserId",
                table: "OutboundEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerSignups_Users_OwnerUserId",
                table: "VolunteerSignups");
        }
    }
}
