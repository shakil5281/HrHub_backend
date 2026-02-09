using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnableEmployeeCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvanceSalaries_Employees_EmployeeId",
                table: "AdvanceSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Bonuses_Employees_EmployeeId",
                table: "Bonuses");

            migrationBuilder.DropForeignKey(
                name: "FK_CounselingRecords_Employees_EmployeeId",
                table: "CounselingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalarySheets_Employees_EmployeeId",
                table: "DailySalarySheets");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShiftRosters_Employees_EmployeeId",
                table: "EmployeeShiftRosters");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveApplications_Employees_EmployeeId",
                table: "LeaveApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlySalarySheets_Employees_EmployeeId",
                table: "MonthlySalarySheets");

            migrationBuilder.DropForeignKey(
                name: "FK_OtDeductions_Employees_EmployeeId",
                table: "OtDeductions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryIncrements_Employees_EmployeeId",
                table: "SalaryIncrements");

            migrationBuilder.DropForeignKey(
                name: "FK_Separations_Employees_EmployeeId",
                table: "Separations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Employees_EmployeeId",
                table: "Transfers");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Attendances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "AttendanceLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_CompanyId",
                table: "Attendances",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_CompanyId",
                table: "AttendanceLogs",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvanceSalaries_Employees_EmployeeId",
                table: "AdvanceSalaries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceLogs_Companies_CompanyId",
                table: "AttendanceLogs",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Companies_CompanyId",
                table: "Attendances",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bonuses_Employees_EmployeeId",
                table: "Bonuses",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CounselingRecords_Employees_EmployeeId",
                table: "CounselingRecords",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalarySheets_Employees_EmployeeId",
                table: "DailySalarySheets",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShiftRosters_Employees_EmployeeId",
                table: "EmployeeShiftRosters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveApplications_Employees_EmployeeId",
                table: "LeaveApplications",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlySalarySheets_Employees_EmployeeId",
                table: "MonthlySalarySheets",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtDeductions_Employees_EmployeeId",
                table: "OtDeductions",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryIncrements_Employees_EmployeeId",
                table: "SalaryIncrements",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Separations_Employees_EmployeeId",
                table: "Separations",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Employees_EmployeeId",
                table: "Transfers",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvanceSalaries_Employees_EmployeeId",
                table: "AdvanceSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceLogs_Companies_CompanyId",
                table: "AttendanceLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Companies_CompanyId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Bonuses_Employees_EmployeeId",
                table: "Bonuses");

            migrationBuilder.DropForeignKey(
                name: "FK_CounselingRecords_Employees_EmployeeId",
                table: "CounselingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySalarySheets_Employees_EmployeeId",
                table: "DailySalarySheets");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeShiftRosters_Employees_EmployeeId",
                table: "EmployeeShiftRosters");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveApplications_Employees_EmployeeId",
                table: "LeaveApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlySalarySheets_Employees_EmployeeId",
                table: "MonthlySalarySheets");

            migrationBuilder.DropForeignKey(
                name: "FK_OtDeductions_Employees_EmployeeId",
                table: "OtDeductions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryIncrements_Employees_EmployeeId",
                table: "SalaryIncrements");

            migrationBuilder.DropForeignKey(
                name: "FK_Separations_Employees_EmployeeId",
                table: "Separations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Employees_EmployeeId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_CompanyId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceLogs_CompanyId",
                table: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "AttendanceLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvanceSalaries_Employees_EmployeeId",
                table: "AdvanceSalaries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bonuses_Employees_EmployeeId",
                table: "Bonuses",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CounselingRecords_Employees_EmployeeId",
                table: "CounselingRecords",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySalarySheets_Employees_EmployeeId",
                table: "DailySalarySheets",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeShiftRosters_Employees_EmployeeId",
                table: "EmployeeShiftRosters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveApplications_Employees_EmployeeId",
                table: "LeaveApplications",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlySalarySheets_Employees_EmployeeId",
                table: "MonthlySalarySheets",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OtDeductions_Employees_EmployeeId",
                table: "OtDeductions",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryIncrements_Employees_EmployeeId",
                table: "SalaryIncrements",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Separations_Employees_EmployeeId",
                table: "Separations",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Employees_EmployeeId",
                table: "Transfers",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
