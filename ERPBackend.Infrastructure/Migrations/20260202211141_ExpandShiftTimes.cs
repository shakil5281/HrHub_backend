using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandShiftTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InTime",
                table: "Shifts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LateLimit",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OutTime",
                table: "Shifts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InTime",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "LateLimit",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "OutTime",
                table: "Shifts");
        }
    }
}
