using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTiffinCountAndTiming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InTime",
                table: "TiffinBills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OutTime",
                table: "TiffinBills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TiffinCount",
                table: "TiffinBills",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InTime",
                table: "TiffinBills");

            migrationBuilder.DropColumn(
                name: "OutTime",
                table: "TiffinBills");

            migrationBuilder.DropColumn(
                name: "TiffinCount",
                table: "TiffinBills");
        }
    }
}
