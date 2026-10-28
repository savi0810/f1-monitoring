using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Laps",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionKey = table.Column<int>(type: "integer", nullable: false),
                    DriverNumber = table.Column<int>(type: "integer", nullable: true),
                    LapNumber = table.Column<int>(type: "integer", nullable: false),
                    LapDuration = table.Column<double>(type: "double precision", nullable: true),
                    Sector1Duration = table.Column<double>(type: "double precision", nullable: true),
                    Sector2Duration = table.Column<double>(type: "double precision", nullable: true),
                    Sector3Duration = table.Column<double>(type: "double precision", nullable: true),
                    IsPitOutLap = table.Column<bool>(type: "boolean", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laps", x => x.EventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Laps_SessionKey",
                table: "Laps",
                column: "SessionKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Laps");
        }
    }
}
