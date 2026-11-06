using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMeetings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    MeetingKey = table.Column<int>(type: "integer", nullable: false),
                    MeetingName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.MeetingKey);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_MeetingKey",
                table: "Sessions",
                column: "MeetingKey");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Meetings_MeetingKey",
                table: "Sessions",
                column: "MeetingKey",
                principalTable: "Meetings",
                principalColumn: "MeetingKey",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Meetings_MeetingKey",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_MeetingKey",
                table: "Sessions");
        }
    }
}
