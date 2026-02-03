using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignationBonusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AttendanceBonus",
                table: "Designations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HolidayBill",
                table: "Designations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NightBill",
                table: "Designations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceBonus",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "HolidayBill",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "NightBill",
                table: "Designations");
        }
    }
}
