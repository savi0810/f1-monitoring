using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionDrivers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionDrivers",
                columns: table => new
                {
                    SessionKey = table.Column<int>(type: "integer", nullable: false),
                    DriverNumber = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    TeamName = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: true),
                    NumberOfLaps = table.Column<int>(type: "integer", nullable: true),
                    DurationJson = table.Column<string>(type: "text", nullable: true),
                    GapToLeaderJson = table.Column<string>(type: "text", nullable: true),
                    DidNotFinish = table.Column<bool>(type: "boolean", nullable: true),
                    DidNotStart = table.Column<bool>(type: "boolean", nullable: true),
                    Disqualified = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionDrivers", x => new { x.SessionKey, x.DriverNumber });
                    table.ForeignKey(
                        name: "FK_SessionDrivers_Sessions_SessionKey",
                        column: x => x.SessionKey,
                        principalTable: "Sessions",
                        principalColumn: "SessionKey",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionDrivers");
        }
    }
}
