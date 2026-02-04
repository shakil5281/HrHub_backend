using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateManpowerRequirementBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManpowerRequirements_Departments_DepartmentId",
                table: "ManpowerRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_ManpowerRequirements_Designations_DesignationId",
                table: "ManpowerRequirements");

            migrationBuilder.AddForeignKey(
                name: "FK_ManpowerRequirements_Departments_DepartmentId",
                table: "ManpowerRequirements",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ManpowerRequirements_Designations_DesignationId",
                table: "ManpowerRequirements",
                column: "DesignationId",
                principalTable: "Designations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManpowerRequirements_Departments_DepartmentId",
                table: "ManpowerRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_ManpowerRequirements_Designations_DesignationId",
                table: "ManpowerRequirements");

            migrationBuilder.AddForeignKey(
                name: "FK_ManpowerRequirements_Departments_DepartmentId",
                table: "ManpowerRequirements",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManpowerRequirements_Designations_DesignationId",
                table: "ManpowerRequirements",
                column: "DesignationId",
                principalTable: "Designations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
