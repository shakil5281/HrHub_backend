using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmployeeIdString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop columns only if they exist (safe for databases that never had these columns)
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Attendances', 'EmployeeIdString') IS NOT NULL
                    ALTER TABLE [Attendances] DROP COLUMN [EmployeeIdString];
            ");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('AttendanceLogs', 'EmployeeIdString') IS NOT NULL
                    ALTER TABLE [AttendanceLogs] DROP COLUMN [EmployeeIdString];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeIdString",
                table: "Attendances",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeIdString",
                table: "AttendanceLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
