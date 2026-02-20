using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToPayrollModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "SalaryIncrements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "MonthlySalarySheets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "DailySalarySheets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Bonuses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "AdvanceSalaries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalaryIncrements_CompanyId",
                table: "SalaryIncrements",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlySalarySheets_CompanyId",
                table: "MonthlySalarySheets",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DailySalarySheets_CompanyId",
                table: "DailySalarySheets",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_CompanyId",
                table: "Bonuses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceSalaries_CompanyId",
                table: "AdvanceSalaries",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvanceSalaries_Companies_CompanyId",
                table: "AdvanceSalaries",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bonuses_Companies_CompanyId",
                table: "Bonuses",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalarySheets_Companies_CompanyId",
                table: "DailySalarySheets",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlySalarySheets_Companies_CompanyId",
                table: "MonthlySalarySheets",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryIncrements_Companies_CompanyId",
                table: "SalaryIncrements",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvanceSalaries_Companies_CompanyId",
                table: "AdvanceSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Bonuses_Companies_CompanyId",
                table: "Bonuses");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalarySheets_Companies_CompanyId",
                table: "DailySalarySheets");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlySalarySheets_Companies_CompanyId",
                table: "MonthlySalarySheets");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryIncrements_Companies_CompanyId",
                table: "SalaryIncrements");

            migrationBuilder.DropIndex(
                name: "IX_SalaryIncrements_CompanyId",
                table: "SalaryIncrements");

            migrationBuilder.DropIndex(
                name: "IX_MonthlySalarySheets_CompanyId",
                table: "MonthlySalarySheets");

            migrationBuilder.DropIndex(
                name: "IX_DailySalarySheets_CompanyId",
                table: "DailySalarySheets");

            migrationBuilder.DropIndex(
                name: "IX_Bonuses_CompanyId",
                table: "Bonuses");

            migrationBuilder.DropIndex(
                name: "IX_AdvanceSalaries_CompanyId",
                table: "AdvanceSalaries");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SalaryIncrements");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "MonthlySalarySheets");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "DailySalarySheets");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Bonuses");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "AdvanceSalaries");
        }
    }
}
