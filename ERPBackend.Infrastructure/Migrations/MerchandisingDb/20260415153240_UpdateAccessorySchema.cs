using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class UpdateAccessorySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ButtonBookings_ProgramOrders_ProgramOrderId",
                table: "ButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ButtonBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "ButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_CareLabelBookings_ProgramOrders_ProgramOrderId",
                table: "CareLabelBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_MainLabelBookings_ProgramOrders_ProgramOrderId",
                table: "MainLabelBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_PolyBookings_ProgramOrders_ProgramOrderId",
                table: "PolyBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_SnapButtonBookings_ProgramOrders_ProgramOrderId",
                table: "SnapButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ThreadBookings_ProgramOrders_ProgramOrderId",
                table: "ThreadBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ZipperBookings_ProgramOrders_ProgramOrderId",
                table: "ZipperBookings");

            migrationBuilder.AddColumn<string>(
                name: "ArticleNo",
                table: "ZipperBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GarmentColor",
                table: "ZipperBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "ZipperBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramSizeBreakdownId",
                table: "ZipperBookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArticleNo",
                table: "ThreadBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GarmentColor",
                table: "ThreadBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "ThreadBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramSizeBreakdownId",
                table: "ThreadBookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArticleNo",
                table: "SnapButtonBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GarmentColor",
                table: "SnapButtonBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "SnapButtonBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramSizeBreakdownId",
                table: "SnapButtonBookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArticleNo",
                table: "PolyBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GarmentColor",
                table: "PolyBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "PolyBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramSizeBreakdownId",
                table: "PolyBookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArticleNo",
                table: "MainLabelBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GarmentColor",
                table: "MainLabelBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "MainLabelBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramSizeBreakdownId",
                table: "MainLabelBookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArticleNo",
                table: "FabricBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GarmentColor",
                table: "FabricBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "FabricBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramSizeBreakdownId",
                table: "FabricBookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArticleNo",
                table: "CareLabelBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GarmentColor",
                table: "CareLabelBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "CareLabelBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgramSizeBreakdownId",
                table: "CareLabelBookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZipperBookings_ProgramSizeBreakdownId",
                table: "ZipperBookings",
                column: "ProgramSizeBreakdownId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadBookings_ProgramSizeBreakdownId",
                table: "ThreadBookings",
                column: "ProgramSizeBreakdownId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapButtonBookings_ProgramSizeBreakdownId",
                table: "SnapButtonBookings",
                column: "ProgramSizeBreakdownId");

            migrationBuilder.CreateIndex(
                name: "IX_PolyBookings_ProgramSizeBreakdownId",
                table: "PolyBookings",
                column: "ProgramSizeBreakdownId");

            migrationBuilder.CreateIndex(
                name: "IX_MainLabelBookings_ProgramSizeBreakdownId",
                table: "MainLabelBookings",
                column: "ProgramSizeBreakdownId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricBookings_ProgramSizeBreakdownId",
                table: "FabricBookings",
                column: "ProgramSizeBreakdownId");

            migrationBuilder.CreateIndex(
                name: "IX_CareLabelBookings_ProgramSizeBreakdownId",
                table: "CareLabelBookings",
                column: "ProgramSizeBreakdownId");

            migrationBuilder.AddForeignKey(
                name: "FK_ButtonBookings_ProgramOrders_ProgramOrderId",
                table: "ButtonBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ButtonBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "ButtonBookings",
                column: "ProgramSizeBreakdownId",
                principalTable: "ProgramSizeBreakdowns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CareLabelBookings_ProgramOrders_ProgramOrderId",
                table: "CareLabelBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CareLabelBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "CareLabelBookings",
                column: "ProgramSizeBreakdownId",
                principalTable: "ProgramSizeBreakdowns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FabricBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "FabricBookings",
                column: "ProgramSizeBreakdownId",
                principalTable: "ProgramSizeBreakdowns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MainLabelBookings_ProgramOrders_ProgramOrderId",
                table: "MainLabelBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MainLabelBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "MainLabelBookings",
                column: "ProgramSizeBreakdownId",
                principalTable: "ProgramSizeBreakdowns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PolyBookings_ProgramOrders_ProgramOrderId",
                table: "PolyBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PolyBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "PolyBookings",
                column: "ProgramSizeBreakdownId",
                principalTable: "ProgramSizeBreakdowns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SnapButtonBookings_ProgramOrders_ProgramOrderId",
                table: "SnapButtonBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SnapButtonBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "SnapButtonBookings",
                column: "ProgramSizeBreakdownId",
                principalTable: "ProgramSizeBreakdowns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadBookings_ProgramOrders_ProgramOrderId",
                table: "ThreadBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "ThreadBookings",
                column: "ProgramSizeBreakdownId",
                principalTable: "ProgramSizeBreakdowns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ZipperBookings_ProgramOrders_ProgramOrderId",
                table: "ZipperBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ZipperBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "ZipperBookings",
                column: "ProgramSizeBreakdownId",
                principalTable: "ProgramSizeBreakdowns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ButtonBookings_ProgramOrders_ProgramOrderId",
                table: "ButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ButtonBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "ButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_CareLabelBookings_ProgramOrders_ProgramOrderId",
                table: "CareLabelBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_CareLabelBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "CareLabelBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_FabricBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "FabricBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_MainLabelBookings_ProgramOrders_ProgramOrderId",
                table: "MainLabelBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_MainLabelBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "MainLabelBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_PolyBookings_ProgramOrders_ProgramOrderId",
                table: "PolyBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_PolyBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "PolyBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_SnapButtonBookings_ProgramOrders_ProgramOrderId",
                table: "SnapButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_SnapButtonBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "SnapButtonBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ThreadBookings_ProgramOrders_ProgramOrderId",
                table: "ThreadBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ThreadBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "ThreadBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ZipperBookings_ProgramOrders_ProgramOrderId",
                table: "ZipperBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ZipperBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "ZipperBookings");

            migrationBuilder.DropIndex(
                name: "IX_ZipperBookings_ProgramSizeBreakdownId",
                table: "ZipperBookings");

            migrationBuilder.DropIndex(
                name: "IX_ThreadBookings_ProgramSizeBreakdownId",
                table: "ThreadBookings");

            migrationBuilder.DropIndex(
                name: "IX_SnapButtonBookings_ProgramSizeBreakdownId",
                table: "SnapButtonBookings");

            migrationBuilder.DropIndex(
                name: "IX_PolyBookings_ProgramSizeBreakdownId",
                table: "PolyBookings");

            migrationBuilder.DropIndex(
                name: "IX_MainLabelBookings_ProgramSizeBreakdownId",
                table: "MainLabelBookings");

            migrationBuilder.DropIndex(
                name: "IX_FabricBookings_ProgramSizeBreakdownId",
                table: "FabricBookings");

            migrationBuilder.DropIndex(
                name: "IX_CareLabelBookings_ProgramSizeBreakdownId",
                table: "CareLabelBookings");

            migrationBuilder.DropColumn(
                name: "ArticleNo",
                table: "ZipperBookings");

            migrationBuilder.DropColumn(
                name: "GarmentColor",
                table: "ZipperBookings");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "ZipperBookings");

            migrationBuilder.DropColumn(
                name: "ProgramSizeBreakdownId",
                table: "ZipperBookings");

            migrationBuilder.DropColumn(
                name: "ArticleNo",
                table: "ThreadBookings");

            migrationBuilder.DropColumn(
                name: "GarmentColor",
                table: "ThreadBookings");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "ThreadBookings");

            migrationBuilder.DropColumn(
                name: "ProgramSizeBreakdownId",
                table: "ThreadBookings");

            migrationBuilder.DropColumn(
                name: "ArticleNo",
                table: "SnapButtonBookings");

            migrationBuilder.DropColumn(
                name: "GarmentColor",
                table: "SnapButtonBookings");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "SnapButtonBookings");

            migrationBuilder.DropColumn(
                name: "ProgramSizeBreakdownId",
                table: "SnapButtonBookings");

            migrationBuilder.DropColumn(
                name: "ArticleNo",
                table: "PolyBookings");

            migrationBuilder.DropColumn(
                name: "GarmentColor",
                table: "PolyBookings");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "PolyBookings");

            migrationBuilder.DropColumn(
                name: "ProgramSizeBreakdownId",
                table: "PolyBookings");

            migrationBuilder.DropColumn(
                name: "ArticleNo",
                table: "MainLabelBookings");

            migrationBuilder.DropColumn(
                name: "GarmentColor",
                table: "MainLabelBookings");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "MainLabelBookings");

            migrationBuilder.DropColumn(
                name: "ProgramSizeBreakdownId",
                table: "MainLabelBookings");

            migrationBuilder.DropColumn(
                name: "ArticleNo",
                table: "FabricBookings");

            migrationBuilder.DropColumn(
                name: "GarmentColor",
                table: "FabricBookings");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "FabricBookings");

            migrationBuilder.DropColumn(
                name: "ProgramSizeBreakdownId",
                table: "FabricBookings");

            migrationBuilder.DropColumn(
                name: "ArticleNo",
                table: "CareLabelBookings");

            migrationBuilder.DropColumn(
                name: "GarmentColor",
                table: "CareLabelBookings");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "CareLabelBookings");

            migrationBuilder.DropColumn(
                name: "ProgramSizeBreakdownId",
                table: "CareLabelBookings");

            migrationBuilder.AddForeignKey(
                name: "FK_ButtonBookings_ProgramOrders_ProgramOrderId",
                table: "ButtonBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ButtonBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "ButtonBookings",
                column: "ProgramSizeBreakdownId",
                principalTable: "ProgramSizeBreakdowns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CareLabelBookings_ProgramOrders_ProgramOrderId",
                table: "CareLabelBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MainLabelBookings_ProgramOrders_ProgramOrderId",
                table: "MainLabelBookings",
                column: "ProgramOrderId",
                principalTable: "ProgramOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PolyBookings_ProgramOrders_ProgramOrderId",
                table: "PolyBookings",
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
    }
}
