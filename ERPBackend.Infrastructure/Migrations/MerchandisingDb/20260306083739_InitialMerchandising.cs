using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class InitialMerchandising : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buyers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeadTime = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brands_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Styles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    StyleNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Season = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FabricType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GSM = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SizeRange = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Styles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Styles_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Styles_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Costings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StyleId = table.Column<int>(type: "int", nullable: false),
                    FabricCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    TrimCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    CMCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    WashCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    PrintCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    EmbroideryCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    PackingCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    OverheadCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    ProfitMargin = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    FOBPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Costings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Costings_Styles_StyleId",
                        column: x => x.StyleId,
                        principalTable: "Styles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SampleRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StyleId = table.Column<int>(type: "int", nullable: false),
                    SampleType = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BuyerFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SampleRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SampleRequests_Styles_StyleId",
                        column: x => x.StyleId,
                        principalTable: "Styles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StyleOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    StyleId = table.Column<int>(type: "int", nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderQuantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StyleOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StyleOrders_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StyleOrders_Styles_StyleId",
                        column: x => x.StyleId,
                        principalTable: "Styles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TechPacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StyleId = table.Column<int>(type: "int", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechPacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechPacks_Styles_StyleId",
                        column: x => x.StyleId,
                        principalTable: "Styles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessoriesBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessoriesBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessoriesBookings_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BOMs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BOMs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BOMs_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FabricBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FabricType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    IssuedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricBookings_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchProductionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Factory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductionLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetPerDay = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchProductionPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchProductionPlans_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderColors_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ShipmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CartonQuantity = table.Column<int>(type: "int", nullable: false),
                    ShippingMethod = table.Column<int>(type: "int", nullable: false),
                    Forwarder = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_StyleOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "StyleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BOMItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BOMId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Consumption = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BOMItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BOMItems_BOMs_BOMId",
                        column: x => x.BOMId,
                        principalTable: "BOMs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderSizeBreakdowns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderColorId = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSizeBreakdowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderSizeBreakdowns_OrderColors_OrderColorId",
                        column: x => x.OrderColorId,
                        principalTable: "OrderColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessoriesBookings_OrderId",
                table: "AccessoriesBookings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BOMItems_BOMId",
                table: "BOMItems",
                column: "BOMId");

            migrationBuilder.CreateIndex(
                name: "IX_BOMs_OrderId",
                table: "BOMs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_BuyerId",
                table: "Brands",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Costings_StyleId",
                table: "Costings",
                column: "StyleId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricBookings_OrderId",
                table: "FabricBookings",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MerchProductionPlans_OrderId",
                table: "MerchProductionPlans",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderColors_OrderId",
                table: "OrderColors",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSizeBreakdowns_OrderColorId",
                table: "OrderSizeBreakdowns",
                column: "OrderColorId");

            migrationBuilder.CreateIndex(
                name: "IX_SampleRequests_StyleId",
                table: "SampleRequests",
                column: "StyleId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_OrderId",
                table: "Shipments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleOrders_BuyerId",
                table: "StyleOrders",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_StyleOrders_StyleId",
                table: "StyleOrders",
                column: "StyleId");

            migrationBuilder.CreateIndex(
                name: "IX_Styles_BrandId",
                table: "Styles",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Styles_BuyerId",
                table: "Styles",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_TechPacks_StyleId",
                table: "TechPacks",
                column: "StyleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessoriesBookings");

            migrationBuilder.DropTable(
                name: "BOMItems");

            migrationBuilder.DropTable(
                name: "Costings");

            migrationBuilder.DropTable(
                name: "FabricBookings");

            migrationBuilder.DropTable(
                name: "MerchProductionPlans");

            migrationBuilder.DropTable(
                name: "OrderSizeBreakdowns");

            migrationBuilder.DropTable(
                name: "SampleRequests");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "TechPacks");

            migrationBuilder.DropTable(
                name: "BOMs");

            migrationBuilder.DropTable(
                name: "OrderColors");

            migrationBuilder.DropTable(
                name: "StyleOrders");

            migrationBuilder.DropTable(
                name: "Styles");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Buyers");
        }
    }
}
