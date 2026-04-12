using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.ProductionDb
{
    /// <inheritdoc />
    public partial class ExpandHourlyProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "H13",
                table: "DailyProductionRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H14",
                table: "DailyProductionRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H15",
                table: "DailyProductionRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H16",
                table: "DailyProductionRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H17",
                table: "DailyProductionRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "H18",
                table: "DailyProductionRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "H13",
                table: "DailyProductionRecords");

            migrationBuilder.DropColumn(
                name: "H14",
                table: "DailyProductionRecords");

            migrationBuilder.DropColumn(
                name: "H15",
                table: "DailyProductionRecords");

            migrationBuilder.DropColumn(
                name: "H16",
                table: "DailyProductionRecords");

            migrationBuilder.DropColumn(
                name: "H17",
                table: "DailyProductionRecords");

            migrationBuilder.DropColumn(
                name: "H18",
                table: "DailyProductionRecords");
        }
    }
}
