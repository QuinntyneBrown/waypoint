using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waypoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonVideoUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DurationSeconds",
                table: "LessonMedia",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureCode",
                table: "LessonMedia",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "LessonMedia",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercent",
                table: "LessonMedia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VideoStatus",
                table: "LessonMedia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "LessonMedia",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VideoTranscodeJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    LeaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastProgressAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTranscodeJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VideoUploadSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExpectedBytes = table.Column<long>(type: "bigint", nullable: false),
                    ChunkSizeBytes = table.Column<int>(type: "int", nullable: false),
                    ChunkCount = table.Column<int>(type: "int", nullable: false),
                    ReceivedChunks = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoUploadSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoTranscodeJobs_LeaseUntil",
                table: "VideoTranscodeJobs",
                column: "LeaseUntil");

            migrationBuilder.CreateIndex(
                name: "IX_VideoTranscodeJobs_MediaId",
                table: "VideoTranscodeJobs",
                column: "MediaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoUploadSessions_ExpiresAt",
                table: "VideoUploadSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_VideoUploadSessions_LessonId",
                table: "VideoUploadSessions",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoTranscodeJobs");

            migrationBuilder.DropTable(
                name: "VideoUploadSessions");

            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "LessonMedia");

            migrationBuilder.DropColumn(
                name: "FailureCode",
                table: "LessonMedia");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "LessonMedia");

            migrationBuilder.DropColumn(
                name: "ProgressPercent",
                table: "LessonMedia");

            migrationBuilder.DropColumn(
                name: "VideoStatus",
                table: "LessonMedia");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "LessonMedia");
        }
    }
}
