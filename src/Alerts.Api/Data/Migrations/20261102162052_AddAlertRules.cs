using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alerts.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverNumber = table.Column<int>(type: "integer", nullable: false),
                    LapTimeThresholdSeconds = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertRules");
        }
    }
}
