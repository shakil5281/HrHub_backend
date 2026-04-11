using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class AddFullAccessorySuite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabelBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabelType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrintDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabelBookings_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackingBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PackingItem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrintDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackingBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackingBookings_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SnapButtonBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SnapType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SnapSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapButtonBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnapButtonBookings_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThreadBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThreadType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColorCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreadBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThreadBookings_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrimBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrimType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrimBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrimBookings_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZipperBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZipperType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZipperSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Length = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZipperBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZipperBookings_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabelBookings_OrderId",
                table: "LabelBookings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingBookings_OrderId",
                table: "PackingBookings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapButtonBookings_OrderId",
                table: "SnapButtonBookings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadBookings_OrderId",
                table: "ThreadBookings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TrimBookings_OrderId",
                table: "TrimBookings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ZipperBookings_OrderId",
                table: "ZipperBookings",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabelBookings");

            migrationBuilder.DropTable(
                name: "PackingBookings");

            migrationBuilder.DropTable(
                name: "SnapButtonBookings");

            migrationBuilder.DropTable(
                name: "ThreadBookings");

            migrationBuilder.DropTable(
                name: "TrimBookings");

            migrationBuilder.DropTable(
                name: "ZipperBookings");
        }
    }
}
