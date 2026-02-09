using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProximityUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OTDeductions_Employees_EmployeeId",
                table: "OTDeductions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OTDeductions",
                table: "OTDeductions");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Proximity",
                table: "Employees");

            migrationBuilder.RenameTable(
                name: "OTDeductions",
                newName: "OtDeductions");

            migrationBuilder.RenameIndex(
                name: "IX_OTDeductions_EmployeeId",
                table: "OtDeductions",
                newName: "IX_OtDeductions_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtDeductions",
                table: "OtDeductions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Proximity_CompanyName",
                table: "Employees",
                columns: new[] { "Proximity", "CompanyName" },
                unique: true,
                filter: "[Proximity] IS NOT NULL AND [CompanyName] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_OtDeductions_Employees_EmployeeId",
                table: "OtDeductions",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtDeductions_Employees_EmployeeId",
                table: "OtDeductions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtDeductions",
                table: "OtDeductions");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Proximity_CompanyName",
                table: "Employees");

            migrationBuilder.RenameTable(
                name: "OtDeductions",
                newName: "OTDeductions");

            migrationBuilder.RenameIndex(
                name: "IX_OtDeductions_EmployeeId",
                table: "OTDeductions",
                newName: "IX_OTDeductions_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OTDeductions",
                table: "OTDeductions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Proximity",
                table: "Employees",
                column: "Proximity",
                unique: true,
                filter: "[Proximity] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_OTDeductions_Employees_EmployeeId",
                table: "OTDeductions",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
