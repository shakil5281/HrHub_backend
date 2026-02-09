using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNamingConventions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NID",
                table: "Employees",
                newName: "Nid");

            migrationBuilder.RenameColumn(
                name: "IsOTEnabled",
                table: "Employees",
                newName: "IsOtEnabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nid",
                table: "Employees",
                newName: "NID");

            migrationBuilder.RenameColumn(
                name: "IsOtEnabled",
                table: "Employees",
                newName: "IsOTEnabled");
        }
    }
}
