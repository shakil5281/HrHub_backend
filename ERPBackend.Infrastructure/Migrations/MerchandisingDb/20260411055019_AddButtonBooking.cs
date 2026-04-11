using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class AddButtonBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ButtonBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ButtonType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ButtonSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ButtonColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ButtonBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ButtonBookings_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ButtonBookings_OrderId",
                table: "ButtonBookings",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ButtonBookings");
        }
    }
}
