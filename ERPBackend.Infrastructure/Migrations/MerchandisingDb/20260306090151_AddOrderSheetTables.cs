using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class AddOrderSheetTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderSheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ProgramNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BuyerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FabricDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSheets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderSheetItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderSheetId = table.Column<int>(type: "int", nullable: false),
                    OldArticleNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewArticleNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackType = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalQty = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSheetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderSheetItems_OrderSheets_OrderSheetId",
                        column: x => x.OrderSheetId,
                        principalTable: "OrderSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderSheetColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderSheetItemId = table.Column<int>(type: "int", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSheetColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderSheetColors_OrderSheetItems_OrderSheetItemId",
                        column: x => x.OrderSheetItemId,
                        principalTable: "OrderSheetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderSheetSizeBreakdowns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderSheetColorId = table.Column<int>(type: "int", nullable: false),
                    SizeM = table.Column<int>(type: "int", nullable: false),
                    SizeL = table.Column<int>(type: "int", nullable: false),
                    SizeXL = table.Column<int>(type: "int", nullable: false),
                    SizeXXL = table.Column<int>(type: "int", nullable: false),
                    SizeXXXL = table.Column<int>(type: "int", nullable: false),
                    Size3XL = table.Column<int>(type: "int", nullable: false),
                    Size4XL = table.Column<int>(type: "int", nullable: false),
                    Size5XL = table.Column<int>(type: "int", nullable: false),
                    Size6XL = table.Column<int>(type: "int", nullable: false),
                    RowTotal = table.Column<int>(type: "int", nullable: false),
                    BuyerPackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSheetSizeBreakdowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderSheetSizeBreakdowns_OrderSheetColors_OrderSheetColorId",
                        column: x => x.OrderSheetColorId,
                        principalTable: "OrderSheetColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderSheetColors_OrderSheetItemId",
                table: "OrderSheetColors",
                column: "OrderSheetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSheetItems_OrderSheetId",
                table: "OrderSheetItems",
                column: "OrderSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSheetSizeBreakdowns_OrderSheetColorId",
                table: "OrderSheetSizeBreakdowns",
                column: "OrderSheetColorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderSheetSizeBreakdowns");

            migrationBuilder.DropTable(
                name: "OrderSheetColors");

            migrationBuilder.DropTable(
                name: "OrderSheetItems");

            migrationBuilder.DropTable(
                name: "OrderSheets");
        }
    }
}
