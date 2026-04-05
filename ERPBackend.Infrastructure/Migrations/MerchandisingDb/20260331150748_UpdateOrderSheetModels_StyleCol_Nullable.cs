using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class UpdateOrderSheetModels_StyleCol_Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StyleId",
                table: "OrderSheetItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "OrderSheetColors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderSheetItems_StyleId",
                table: "OrderSheetItems",
                column: "StyleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSheetColors_ColorId",
                table: "OrderSheetColors",
                column: "ColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderSheetColors_FabricColorPantones_ColorId",
                table: "OrderSheetColors",
                column: "ColorId",
                principalTable: "FabricColorPantones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderSheetItems_Styles_StyleId",
                table: "OrderSheetItems",
                column: "StyleId",
                principalTable: "Styles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderSheetColors_FabricColorPantones_ColorId",
                table: "OrderSheetColors");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderSheetItems_Styles_StyleId",
                table: "OrderSheetItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderSheetItems_StyleId",
                table: "OrderSheetItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderSheetColors_ColorId",
                table: "OrderSheetColors");

            migrationBuilder.DropColumn(
                name: "StyleId",
                table: "OrderSheetItems");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "OrderSheetColors");
        }
    }
}
