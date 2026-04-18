using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class UpdateButtonBookingSizeBreakdownId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderReference",
                table: "ZipperBookings");

            migrationBuilder.DropColumn(
                name: "OrderReference",
                table: "ThreadBookings");

            migrationBuilder.DropColumn(
                name: "OrderReference",
                table: "SnapButtonBookings");

            migrationBuilder.DropColumn(
                name: "OrderReference",
                table: "PolyBookings");

            migrationBuilder.DropColumn(
                name: "OrderReference",
                table: "MainLabelBookings");

            migrationBuilder.DropColumn(
                name: "OrderReference",
                table: "FabricBookings");

            migrationBuilder.DropColumn(
                name: "OrderReference",
                table: "CareLabelBookings");

            migrationBuilder.DropColumn(
                name: "OrderReference",
                table: "ButtonBookings");

            migrationBuilder.CreateIndex(
                name: "IX_ButtonBookings_ProgramSizeBreakdownId",
                table: "ButtonBookings",
                column: "ProgramSizeBreakdownId");

            migrationBuilder.AddForeignKey(
                name: "FK_ButtonBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "ButtonBookings",
                column: "ProgramSizeBreakdownId",
                principalTable: "ProgramSizeBreakdowns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ButtonBookings_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                table: "ButtonBookings");

            migrationBuilder.DropIndex(
                name: "IX_ButtonBookings_ProgramSizeBreakdownId",
                table: "ButtonBookings");

            migrationBuilder.AddColumn<string>(
                name: "OrderReference",
                table: "ZipperBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderReference",
                table: "ThreadBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderReference",
                table: "SnapButtonBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderReference",
                table: "PolyBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderReference",
                table: "MainLabelBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderReference",
                table: "FabricBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderReference",
                table: "CareLabelBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderReference",
                table: "ButtonBookings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
