using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrganogramRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Designations_Sections_SectionId",
                table: "Designations");

            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Sections_SectionId",
                table: "Lines");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Departments_DepartmentId",
                table: "Sections");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Sections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Lines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Lines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Designations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Designations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sections_CompanyId",
                table: "Sections",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Lines_CompanyId",
                table: "Lines",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Lines_DepartmentId",
                table: "Lines",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Designations_CompanyId",
                table: "Designations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Designations_DepartmentId",
                table: "Designations",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Designations_Companies_CompanyId",
                table: "Designations",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Designations_Departments_DepartmentId",
                table: "Designations",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Designations_Sections_SectionId",
                table: "Designations",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Companies_CompanyId",
                table: "Lines",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Departments_DepartmentId",
                table: "Lines",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Sections_SectionId",
                table: "Lines",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Companies_CompanyId",
                table: "Sections",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Departments_DepartmentId",
                table: "Sections",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Designations_Companies_CompanyId",
                table: "Designations");

            migrationBuilder.DropForeignKey(
                name: "FK_Designations_Departments_DepartmentId",
                table: "Designations");

            migrationBuilder.DropForeignKey(
                name: "FK_Designations_Sections_SectionId",
                table: "Designations");

            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Companies_CompanyId",
                table: "Lines");

            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Departments_DepartmentId",
                table: "Lines");

            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Sections_SectionId",
                table: "Lines");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Companies_CompanyId",
                table: "Sections");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Departments_DepartmentId",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Sections_CompanyId",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Lines_CompanyId",
                table: "Lines");

            migrationBuilder.DropIndex(
                name: "IX_Lines_DepartmentId",
                table: "Lines");

            migrationBuilder.DropIndex(
                name: "IX_Designations_CompanyId",
                table: "Designations");

            migrationBuilder.DropIndex(
                name: "IX_Designations_DepartmentId",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Lines");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Lines");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Designations");

            migrationBuilder.AddForeignKey(
                name: "FK_Designations_Sections_SectionId",
                table: "Designations",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Sections_SectionId",
                table: "Lines",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Departments_DepartmentId",
                table: "Sections",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
