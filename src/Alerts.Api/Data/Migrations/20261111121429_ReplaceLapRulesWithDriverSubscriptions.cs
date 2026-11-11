using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alerts.Api.Data.Migrations
{
    public partial class ReplaceLapRulesWithDriverSubscriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertRules");

            migrationBuilder.CreateTable(
                name: "CompletedRaces",
                columns: table => new
                {
                    SessionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedRaces", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "DriverSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedRaces_RecordedAt",
                table: "CompletedRaces",
                column: "RecordedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DriverSubscriptions_DriverId",
                table: "DriverSubscriptions",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverSubscriptions_UserId_DriverId",
                table: "DriverSubscriptions",
                columns: new[] { "UserId", "DriverId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletedRaces");

            migrationBuilder.DropTable(
                name: "DriverSubscriptions");

            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverNumber = table.Column<int>(type: "integer", nullable: false),
                    LapTimeThresholdSeconds = table.Column<double>(type: "double precision", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                });
        }
    }
}
