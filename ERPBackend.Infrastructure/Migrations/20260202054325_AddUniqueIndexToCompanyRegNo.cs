using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToCompanyRegNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Companies_RegistrationNo",
                table: "Companies",
                column: "RegistrationNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_RegistrationNo",
                table: "Companies");
        }
    }
}
