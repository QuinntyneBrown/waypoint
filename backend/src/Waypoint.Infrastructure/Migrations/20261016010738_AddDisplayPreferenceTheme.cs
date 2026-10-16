using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waypoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayPreferenceTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "DisplayPreferences",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "system");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Theme",
                table: "DisplayPreferences");
        }
    }
}
