using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBilingualNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Thanas",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Sections",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "PostOffices",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Lines",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Divisions",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Districts",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Designations",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Departments",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Countries",
                newName: "NameEn");

            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "Thanas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "Sections",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "PostOffices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "Lines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "Divisions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "Districts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "Designations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "Departments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameBn",
                table: "Countries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "Thanas");

            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "PostOffices");

            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "Lines");

            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "NameBn",
                table: "Countries");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Thanas",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Sections",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "PostOffices",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Lines",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Divisions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Districts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Designations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Departments",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Countries",
                newName: "Name");
        }
    }
}
