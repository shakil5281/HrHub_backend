using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmployeeIdInAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceLogs_Employees_EmployeeId",
                table: "AttendanceLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_EmployeeId",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceLogs_EmployeeId",
                table: "AttendanceLogs");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "Attendances",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCard",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "AttendanceLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCard",
                table: "AttendanceLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE Attendances SET EmployeeCard = CAST(EmployeeId AS int)");
            migrationBuilder.Sql("UPDATE AttendanceLogs SET EmployeeCard = CAST(EmployeeId AS int)");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeCard",
                table: "Attendances",
                column: "EmployeeCard");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_EmployeeCard",
                table: "AttendanceLogs",
                column: "EmployeeCard");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceLogs_Employees_EmployeeCard",
                table: "AttendanceLogs",
                column: "EmployeeCard",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Employees_EmployeeCard",
                table: "Attendances",
                column: "EmployeeCard",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceLogs_Employees_EmployeeCard",
                table: "AttendanceLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Employees_EmployeeCard",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_EmployeeCard",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceLogs_EmployeeCard",
                table: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "EmployeeCard",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "EmployeeCard",
                table: "AttendanceLogs");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "AttendanceLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId",
                table: "Attendances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLogs_EmployeeId",
                table: "AttendanceLogs",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceLogs_Employees_EmployeeId",
                table: "AttendanceLogs",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
