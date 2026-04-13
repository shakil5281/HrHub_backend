using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class RelationalModularRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ButtonBookings_OrderSheets_ProgramId",
                table: "ButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_FabricBookings_StyleOrders_OrderId",
                table: "FabricBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_SnapButtonBookings_StyleOrders_OrderId",
                table: "SnapButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Styles_Brands_BrandId",
                table: "Styles");

            migrationBuilder.DropForeignKey(
                name: "FK_Styles_Buyers_BuyerId",
                table: "Styles");

            migrationBuilder.DropForeignKey(
                name: "FK_ThreadBookings_StyleOrders_OrderId",
                table: "ThreadBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ZipperBookings_StyleOrders_OrderId",
                table: "ZipperBookings");

            migrationBuilder.DropTable(
                name: "AccessoriesBookings");

            migrationBuilder.DropTable(
                name: "AccessoriesConsumptions");

            migrationBuilder.DropTable(
                name: "BOMItems");

            migrationBuilder.DropTable(
                name: "ExportItems");

            migrationBuilder.DropTable(
                name: "FabricConsumptions");

            migrationBuilder.DropTable(
                name: "LabelBookings");

            migrationBuilder.DropTable(
                name: "MerchProductionPlans");

            migrationBuilder.DropTable(
                name: "OrderSheetSizeBreakdowns");

            migrationBuilder.DropTable(
                name: "OrderSizeBreakdowns");

            migrationBuilder.DropTable(
                name: "PackingBookings");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "SubContractOrders");

            migrationBuilder.DropTable(
                name: "TrimBookings");

            migrationBuilder.DropTable(
                name: "YarnInventories");

            migrationBuilder.DropTable(
                name: "BOMs");

            migrationBuilder.DropTable(
                name: "OrderSheetColors");

            migrationBuilder.DropTable(
                name: "OrderColors");

            migrationBuilder.DropTable(
                name: "OrderSheetItems");

            migrationBuilder.DropTable(
                name: "StyleOrders");

            migrationBuilder.DropTable(
                name: "OrderSheets");

            migrationBuilder.DropIndex(
                name: "IX_ButtonBookings_ProgramId",
                table: "ButtonBookings");

            migrationBuilder.DropColumn(
                name: "IssuedQuantity",
                table: "FabricBookings");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                table: "ButtonBookings");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "ZipperBookings",
                newName: "ProgramOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ZipperBookings_OrderId",
                table: "ZipperBookings",
                newName: "IX_ZipperBookings_ProgramOrderId");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "ThreadBookings",
                newName: "ProgramOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ThreadBookings_OrderId",
                table: "ThreadBookings",
                newName: "IX_ThreadBookings_ProgramOrderId");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "SnapButtonBookings",
                newName: "ProgramOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_SnapButtonBookings_OrderId",
                table: "SnapButtonBookings",
                newName: "IX_SnapButtonBookings_ProgramOrderId");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "FabricBookings",
                newName: "ProgramOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_FabricBookings_OrderId",
                table: "FabricBookings",
                newName: "IX_FabricBookings_ProgramOrderId");

            migrationBuilder.AddColumn<int>(
                name: "ProgramOrderId",
                table: "ButtonBookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProgramOrders",
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
                    table.PrimaryKey("PK_ProgramOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareLabelBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrintDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramOrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareLabelBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CareLabelBookings_ProgramOrders_ProgramOrderId",
                        column: x => x.ProgramOrderId,
                        principalTable: "ProgramOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MainLabelBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrintDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramOrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainLabelBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MainLabelBookings_ProgramOrders_ProgramOrderId",
                        column: x => x.ProgramOrderId,
                        principalTable: "ProgramOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolyBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolyType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrintDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramOrderId = table.Column<int>(type: "int", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolyBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolyBookings_ProgramOrders_ProgramOrderId",
                        column: x => x.ProgramOrderId,
                        principalTable: "ProgramOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramOrderId = table.Column<int>(type: "int", nullable: false),
                    StyleId = table.Column<int>(type: "int", nullable: true),
                    OldArticleNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewArticleNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackType = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalQty = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramArticles_ProgramOrders_ProgramOrderId",
                        column: x => x.ProgramOrderId,
                        principalTable: "ProgramOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramArticles_Styles_StyleId",
                        column: x => x.StyleId,
                        principalTable: "Styles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProgramColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramArticleId = table.Column<int>(type: "int", nullable: false),
                    ColorId = table.Column<int>(type: "int", nullable: true),
                    ColorName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramColors_FabricColorPantones_ColorId",
                        column: x => x.ColorId,
                        principalTable: "FabricColorPantones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProgramColors_ProgramArticles_ProgramArticleId",
                        column: x => x.ProgramArticleId,
                        principalTable: "ProgramArticles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramSizeBreakdowns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramColorId = table.Column<int>(type: "int", nullable: false),
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
                    BuyerPackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ButtonColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ButtonQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramSizeBreakdowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramSizeBreakdowns_ProgramColors_ProgramColorId",
                        column: x => x.ProgramColorId,
                        principalTable: "ProgramColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ButtonBookings_ProgramOrderId",
                table: "ButtonBookings",
                column: "ProgramOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CareLabelBookings_ProgramOrderId",
                table: "CareLabelBookings",
                column: "ProgramOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MainLabelBookings_ProgramOrderId",
                table: "MainLabelBookings",
                column: "ProgramOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PolyBookings_ProgramOrderId",
                table: "PolyBookings",
                column: "ProgramOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramArticles_ProgramOrderId",
                table: "ProgramArticles",
                column: "ProgramOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramArticles_StyleId",
                table: "ProgramArticles",
                column: "StyleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramColors_ColorId",
                table: "ProgramColors",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramColors_ProgramArticleId",
                table: "ProgramColors",
                column: "ProgramArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramOrders_CompanyId_ProgramNumber",
                table: "ProgramOrders",
                columns: new[] { "CompanyId", "ProgramNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramSizeBreakdowns_ProgramColorId",
                table: "ProgramSizeBreakdowns",
                column: "ProgramColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ButtonBookings_ProgramOrders_ProgramOrderId",
                table: "ButtonBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FabricBookings_ProgramOrders_ProgramOrderId",
                table: "FabricBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SnapButtonBookings_ProgramOrders_ProgramOrderId",
                table: "SnapButtonBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Styles_Brands_BrandId",
                table: "Styles",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Styles_Buyers_BuyerId",
                table: "Styles",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadBookings_ProgramOrders_ProgramOrderId",
                table: "ThreadBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ZipperBookings_ProgramOrders_ProgramOrderId",
                table: "ZipperBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ButtonBookings_ProgramOrders_ProgramOrderId",
                table: "ButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_FabricBookings_ProgramOrders_ProgramOrderId",
                table: "FabricBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_SnapButtonBookings_ProgramOrders_ProgramOrderId",
                table: "SnapButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Styles_Brands_BrandId",
                table: "Styles");

            migrationBuilder.DropForeignKey(
                name: "FK_Styles_Buyers_BuyerId",
                table: "Styles");

            migrationBuilder.DropForeignKey(
                name: "FK_ThreadBookings_ProgramOrders_ProgramOrderId",
                table: "ThreadBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ZipperBookings_ProgramOrders_ProgramOrderId",
                table: "ZipperBookings");

            migrationBuilder.DropTable(
                name: "CareLabelBookings");

            migrationBuilder.DropTable(
                name: "MainLabelBookings");

            migrationBuilder.DropTable(
                name: "PolyBookings");

            migrationBuilder.DropTable(
                name: "ProgramSizeBreakdowns");

            migrationBuilder.DropTable(
                name: "ProgramColors");

            migrationBuilder.DropTable(
                name: "ProgramArticles");

            migrationBuilder.DropTable(
                name: "ProgramOrders");

            migrationBuilder.DropIndex(
                name: "IX_ButtonBookings_ProgramOrderId",
                table: "ButtonBookings");

            migrationBuilder.DropColumn(
                name: "ProgramOrderId",
                table: "ButtonBookings");

            migrationBuilder.RenameColumn(
                name: "ProgramOrderId",
                table: "ZipperBookings",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ZipperBookings_ProgramOrderId",
                table: "ZipperBookings",
                newName: "IX_ZipperBookings_OrderId");

            migrationBuilder.RenameColumn(
                name: "ProgramOrderId",
                table: "ThreadBookings",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ThreadBookings_ProgramOrderId",
                table: "ThreadBookings",
                newName: "IX_ThreadBookings_OrderId");

            migrationBuilder.RenameColumn(
                name: "ProgramOrderId",
                table: "SnapButtonBookings",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_SnapButtonBookings_ProgramOrderId",
                table: "SnapButtonBookings",
                newName: "IX_SnapButtonBookings_OrderId");

            migrationBuilder.RenameColumn(
                name: "ProgramOrderId",
                table: "FabricBookings",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_FabricBookings_ProgramOrderId",
                table: "FabricBookings",
                newName: "IX_FabricBookings_OrderId");

            migrationBuilder.AddColumn<decimal>(
                name: "IssuedQuantity",
                table: "FabricBookings",
                type: "decimal(18,4)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProgramId",
                table: "ButtonBookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccessoriesConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ConsumptionPerPcs = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StyleId = table.Column<int>(type: "int", nullable: false),
                    WastagePercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessoriesConsumptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExportItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HSCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FabricConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ComponentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsumptionPerPcs = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    StyleId = table.Column<int>(type: "int", nullable: false),
                    TotalConsumption = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WastagePercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricConsumptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderSheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    BuyerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FabricDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSheets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StyleOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    StyleId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderQuantity = table.Column<int>(type: "int", nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
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
                name: "SubContractOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubContractOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YarnInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Composition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Count = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StockQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YarnType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YarnInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderSheetItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderSheetId = table.Column<int>(type: "int", nullable: false),
                    StyleId = table.Column<int>(type: "int", nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewArticleNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldArticleNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackType = table.Column<int>(type: "int", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_OrderSheetItems_Styles_StyleId",
                        column: x => x.StyleId,
                        principalTable: "Styles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccessoriesBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "LabelBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LabelType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrintDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "MerchProductionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Factory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductionLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetPerDay = table.Column<int>(type: "int", nullable: false)
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
                name: "PackingBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PackingItem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrintDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CartonQuantity = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Forwarder = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShipmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShippingMethod = table.Column<int>(type: "int", nullable: false)
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
                name: "TrimBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrimType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                name: "OrderSheetColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColorId = table.Column<int>(type: "int", nullable: true),
                    OrderSheetItemId = table.Column<int>(type: "int", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSheetColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderSheetColors_FabricColorPantones_ColorId",
                        column: x => x.ColorId,
                        principalTable: "FabricColorPantones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderSheetColors_OrderSheetItems_OrderSheetItemId",
                        column: x => x.OrderSheetItemId,
                        principalTable: "OrderSheetItems",
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
                    Consumption = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 2, nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "OrderSheetSizeBreakdowns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderSheetColorId = table.Column<int>(type: "int", nullable: false),
                    ButtonColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ButtonQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BuyerPackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowTotal = table.Column<int>(type: "int", nullable: false),
                    Size3XL = table.Column<int>(type: "int", nullable: false),
                    Size4XL = table.Column<int>(type: "int", nullable: false),
                    Size5XL = table.Column<int>(type: "int", nullable: false),
                    Size6XL = table.Column<int>(type: "int", nullable: false),
                    SizeL = table.Column<int>(type: "int", nullable: false),
                    SizeM = table.Column<int>(type: "int", nullable: false),
                    SizeXL = table.Column<int>(type: "int", nullable: false),
                    SizeXXL = table.Column<int>(type: "int", nullable: false),
                    SizeXXXL = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_ButtonBookings_ProgramId",
                table: "ButtonBookings",
                column: "ProgramId");

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
                name: "IX_LabelBookings_OrderId",
                table: "LabelBookings",
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
                name: "IX_OrderSheetColors_ColorId",
                table: "OrderSheetColors",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSheetColors_OrderSheetItemId",
                table: "OrderSheetColors",
                column: "OrderSheetItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSheetItems_OrderSheetId",
                table: "OrderSheetItems",
                column: "OrderSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSheetItems_StyleId",
                table: "OrderSheetItems",
                column: "StyleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSheets_CompanyId_ProgramNumber",
                table: "OrderSheets",
                columns: new[] { "CompanyId", "ProgramNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderSheetSizeBreakdowns_OrderSheetColorId",
                table: "OrderSheetSizeBreakdowns",
                column: "OrderSheetColorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSizeBreakdowns_OrderColorId",
                table: "OrderSizeBreakdowns",
                column: "OrderColorId");

            migrationBuilder.CreateIndex(
                name: "IX_PackingBookings_OrderId",
                table: "PackingBookings",
                column: "OrderId");

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
                name: "IX_TrimBookings_OrderId",
                table: "TrimBookings",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ButtonBookings_OrderSheets_ProgramId",
                table: "ButtonBookings",
                column: "ProgramId",
                principalTable: "OrderSheets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FabricBookings_StyleOrders_OrderId",
                table: "FabricBookings",
                column: "OrderId",
                principalTable: "StyleOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SnapButtonBookings_StyleOrders_OrderId",
                table: "SnapButtonBookings",
                column: "OrderId",
                principalTable: "StyleOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Styles_Brands_BrandId",
                table: "Styles",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Styles_Buyers_BuyerId",
                table: "Styles",
                column: "BuyerId",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadBookings_StyleOrders_OrderId",
                table: "ThreadBookings",
                column: "OrderId",
                principalTable: "StyleOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ZipperBookings_StyleOrders_OrderId",
                table: "ZipperBookings",
                column: "OrderId",
                principalTable: "StyleOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
