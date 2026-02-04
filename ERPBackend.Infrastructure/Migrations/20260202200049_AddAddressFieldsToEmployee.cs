using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressFieldsToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PermanentAddressBn",
                table: "Employees",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PermanentDistrictId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PermanentDivisionId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PermanentPostOfficeId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentPostalCode",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PermanentThanaId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PresentAddressBn",
                table: "Employees",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PresentDistrictId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PresentDivisionId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PresentPostOfficeId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PresentPostalCode",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PresentThanaId",
                table: "Employees",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PermanentAddressBn",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PermanentDistrictId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PermanentDivisionId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PermanentPostOfficeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PermanentPostalCode",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PermanentThanaId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PresentAddressBn",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PresentDistrictId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PresentDivisionId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PresentPostOfficeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PresentPostalCode",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PresentThanaId",
                table: "Employees");
        }
    }
}
