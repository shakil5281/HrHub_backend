using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserCompanyRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUserCompany",
                columns: table => new
                {
                    AssignedCompaniesId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserCompany", x => new { x.AssignedCompaniesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserCompany_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserCompany_Companies_AssignedCompaniesId",
                        column: x => x.AssignedCompaniesId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserCompany_UsersId",
                table: "ApplicationUserCompany",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserCompany");
        }
    }
}
