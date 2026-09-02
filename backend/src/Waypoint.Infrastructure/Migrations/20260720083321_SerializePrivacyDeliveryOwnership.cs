using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waypoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SerializePrivacyDeliveryOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "VolunteerSignups",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivacyRemovalConfirmation",
                table: "OutboundEmails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RecipientUserId",
                table: "OutboundEmails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "DonationPledges",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "ContactRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicantUserId",
                table: "Applications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GuardianUserId",
                table: "Applications",
                type: "uniqueidentifier",
                nullable: true);

            // Existing approved applications already have an authoritative youth-account link.
            // Carry its relational guardian association forward without inferring identity from email.
            migrationBuilder.Sql("""
                UPDATE applicationRow
                SET GuardianUserId = youth.GuardianUserId
                FROM Applications AS applicationRow
                INNER JOIN Users AS youth ON youth.Id = applicationRow.CreatedYouthUserId
                WHERE applicationRow.GuardianUserId IS NULL
                  AND youth.GuardianUserId IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerSignups_OwnerUserId",
                table: "VolunteerSignups",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboundEmails_RecipientUserId",
                table: "OutboundEmails",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationPledges_OwnerUserId",
                table: "DonationPledges",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_OwnerUserId",
                table: "ContactRequests",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicantUserId",
                table: "Applications",
                column: "ApplicantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CreatedYouthUserId",
                table: "Applications",
                column: "CreatedYouthUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_GuardianUserId",
                table: "Applications",
                column: "GuardianUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VolunteerSignups_OwnerUserId",
                table: "VolunteerSignups");

            migrationBuilder.DropIndex(
                name: "IX_OutboundEmails_RecipientUserId",
                table: "OutboundEmails");

            migrationBuilder.DropIndex(
                name: "IX_DonationPledges_OwnerUserId",
                table: "DonationPledges");

            migrationBuilder.DropIndex(
                name: "IX_ContactRequests_OwnerUserId",
                table: "ContactRequests");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ApplicantUserId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_CreatedYouthUserId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_GuardianUserId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "VolunteerSignups");

            migrationBuilder.DropColumn(
                name: "IsPrivacyRemovalConfirmation",
                table: "OutboundEmails");

            migrationBuilder.DropColumn(
                name: "RecipientUserId",
                table: "OutboundEmails");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "DonationPledges");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "ContactRequests");

            migrationBuilder.DropColumn(
                name: "ApplicantUserId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "GuardianUserId",
                table: "Applications");
        }
    }
}
