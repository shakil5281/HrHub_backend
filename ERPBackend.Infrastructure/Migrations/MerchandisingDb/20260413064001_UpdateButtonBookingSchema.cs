using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class UpdateButtonBookingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ButtonColor",
                table: "ProgramSizeBreakdowns");

            migrationBuilder.DropColumn(
                name: "ButtonQty",
                table: "ProgramSizeBreakdowns");

            migrationBuilder.AddColumn<string>(
                name: "ArticleNo",
                table: "ButtonBookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "ButtonBookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArticleNo",
                table: "ButtonBookings");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "ButtonBookings");

            migrationBuilder.AddColumn<string>(
                name: "ButtonColor",
                table: "ProgramSizeBreakdowns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ButtonQty",
                table: "ProgramSizeBreakdowns",
                type: "decimal(18,2)",
                precision: 18,
                scale: 4,
                nullable: true);
        }
    }
}
