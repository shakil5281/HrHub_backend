using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShiftFields_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LateLimit",
                table: "Shifts");

            migrationBuilder.AddColumn<string>(
                name: "LateInTime",
                table: "Shifts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LunchHour",
                table: "Shifts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LunchTimeStart",
                table: "Shifts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Shifts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Weekends",
                table: "Shifts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LateInTime",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "LunchHour",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "LunchTimeStart",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Weekends",
                table: "Shifts");

            migrationBuilder.AddColumn<int>(
                name: "LateLimit",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
