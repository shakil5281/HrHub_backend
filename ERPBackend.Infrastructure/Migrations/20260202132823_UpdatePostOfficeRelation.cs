using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePostOfficeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostOffices_Thanas_ThanaId",
                table: "PostOffices");

            migrationBuilder.RenameColumn(
                name: "ThanaId",
                table: "PostOffices",
                newName: "DistrictId");

            migrationBuilder.RenameIndex(
                name: "IX_PostOffices_ThanaId",
                table: "PostOffices",
                newName: "IX_PostOffices_DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostOffices_Districts_DistrictId",
                table: "PostOffices",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostOffices_Districts_DistrictId",
                table: "PostOffices");

            migrationBuilder.RenameColumn(
                name: "DistrictId",
                table: "PostOffices",
                newName: "ThanaId");

            migrationBuilder.RenameIndex(
                name: "IX_PostOffices_DistrictId",
                table: "PostOffices",
                newName: "IX_PostOffices_ThanaId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostOffices_Thanas_ThanaId",
                table: "PostOffices",
                column: "ThanaId",
                principalTable: "Thanas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
