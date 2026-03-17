using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdvanceSalaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AbsentDays",
                table: "AdvanceSalaries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AbsentDeduction",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BasicSalary",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FoodAllowance",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossSalary",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HouseRent",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MedicalAllowance",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetPayable",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OTAmount",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OTHours",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OTRate",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PresentDays",
                table: "AdvanceSalaries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPayableWages",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransportAllowance",
                table: "AdvanceSalaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbsentDays",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "AbsentDeduction",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "BasicSalary",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "FoodAllowance",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "GrossSalary",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "HouseRent",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "MedicalAllowance",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "NetPayable",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "OTAmount",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "OTHours",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "OTRate",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "PresentDays",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "TotalPayableWages",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "TransportAllowance",
                table: "AdvanceSalaries");
        }
    }
}
