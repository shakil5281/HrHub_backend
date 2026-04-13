using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class AddBuyerIdToProgramOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuyerId",
                table: "ProgramOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramOrders_BuyerId",
                table: "ProgramOrders",
                column: "BuyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramOrders_Buyers_BuyerId",
                table: "ProgramOrders",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgramOrders_Buyers_BuyerId",
                table: "ProgramOrders");

            migrationBuilder.DropIndex(
                name: "IX_ProgramOrders_BuyerId",
                table: "ProgramOrders");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "ProgramOrders");
        }
    }
}
