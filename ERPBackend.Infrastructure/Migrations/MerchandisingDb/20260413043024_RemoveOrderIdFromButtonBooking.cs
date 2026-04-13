using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class RemoveOrderIdFromButtonBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ButtonBookings_StyleOrders_OrderId",
                table: "ButtonBookings");

            migrationBuilder.DropIndex(
                name: "IX_ButtonBookings_OrderId",
                table: "ButtonBookings");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "ButtonBookings");

            migrationBuilder.AddColumn<int>(
                name: "ProgramId",
                table: "ButtonBookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ButtonBookings_ProgramId",
                table: "ButtonBookings",
                column: "ProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_ButtonBookings_OrderSheets_ProgramId",
                table: "ButtonBookings",
                column: "ProgramId",
                principalTable: "OrderSheets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ButtonBookings_OrderSheets_ProgramId",
                table: "ButtonBookings");

            migrationBuilder.DropIndex(
                name: "IX_ButtonBookings_ProgramId",
                table: "ButtonBookings");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "ButtonBookings");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "ButtonBookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ButtonBookings_OrderId",
                table: "ButtonBookings",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ButtonBookings_StyleOrders_OrderId",
                table: "ButtonBookings",
                column: "OrderId",
                principalTable: "StyleOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
