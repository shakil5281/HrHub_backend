using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialBreakToShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasSpecialBreak",
                table: "Shifts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SpecialBreakDates",
                table: "Shifts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialBreakEnd",
                table: "Shifts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialBreakStart",
                table: "Shifts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasSpecialBreak",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "SpecialBreakDates",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "SpecialBreakEnd",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "SpecialBreakStart",
                table: "Shifts");
        }
    }
}
