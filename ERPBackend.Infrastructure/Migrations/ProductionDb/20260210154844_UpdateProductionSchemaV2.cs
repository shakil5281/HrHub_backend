using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.ProductionDb
{
    /// <inheritdoc />
    public partial class UpdateProductionSchemaV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Productions");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Productions");

            migrationBuilder.DropColumn(
                name: "Product",
                table: "Productions");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Productions");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Productions",
                newName: "OrderQty");

            migrationBuilder.RenameColumn(
                name: "Line",
                table: "Productions",
                newName: "Item");

            migrationBuilder.RenameColumn(
                name: "BatchId",
                table: "Productions",
                newName: "StyleNo");

            migrationBuilder.RenameIndex(
                name: "IX_Productions_BatchId",
                table: "Productions",
                newName: "IX_Productions_StyleNo");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Productions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Buyer",
                table: "Productions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProgramCode",
                table: "Productions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "Productions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ProductionColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionId = table.Column<int>(type: "int", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionColors_Productions_ProductionId",
                        column: x => x.ProductionId,
                        principalTable: "Productions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionColors_ProductionId",
                table: "ProductionColors",
                column: "ProductionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionColors");

            migrationBuilder.DropColumn(
                name: "Buyer",
                table: "Productions");

            migrationBuilder.DropColumn(
                name: "ProgramCode",
                table: "Productions");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "Productions");

            migrationBuilder.RenameColumn(
                name: "StyleNo",
                table: "Productions",
                newName: "BatchId");

            migrationBuilder.RenameColumn(
                name: "OrderQty",
                table: "Productions",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "Item",
                table: "Productions",
                newName: "Line");

            migrationBuilder.RenameIndex(
                name: "IX_Productions_StyleNo",
                table: "Productions",
                newName: "IX_Productions_BatchId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Productions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Productions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Productions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Product",
                table: "Productions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Productions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
