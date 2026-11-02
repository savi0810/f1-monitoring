using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionImportState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ImportCompletedAt",
                table: "Sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportError",
                table: "Sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImportId",
                table: "Sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImportStatus",
                table: "Sessions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportCompletedAt",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ImportError",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ImportId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ImportStatus",
                table: "Sessions");
        }
    }
}
