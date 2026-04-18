using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class AddButtonColorRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ButtonColorId",
                table: "ButtonBookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ButtonBookings_ButtonColorId",
                table: "ButtonBookings",
                column: "ButtonColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ButtonBookings_FabricColorPantones_ButtonColorId",
                table: "ButtonBookings",
                column: "ButtonColorId",
                principalTable: "FabricColorPantones",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ButtonBookings_FabricColorPantones_ButtonColorId",
                table: "ButtonBookings");

            migrationBuilder.DropIndex(
                name: "IX_ButtonBookings_ButtonColorId",
                table: "ButtonBookings");

            migrationBuilder.DropColumn(
                name: "ButtonColorId",
                table: "ButtonBookings");
        }
    }
}
