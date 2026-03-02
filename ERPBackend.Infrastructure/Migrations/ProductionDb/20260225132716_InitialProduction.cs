using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.ProductionDb
{
    /// <inheritdoc />
    public partial class InitialProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductionLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SL = table.Column<int>(type: "int", nullable: false),
                    LineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Buyer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderQty = table.Column<int>(type: "int", nullable: false),
                    StyleNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Item = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionId = table.Column<int>(type: "int", nullable: false),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    TotalTarget = table.Column<int>(type: "int", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionAssignments_ProductionLines_LineId",
                        column: x => x.LineId,
                        principalTable: "ProductionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionAssignments_Productions_ProductionId",
                        column: x => x.ProductionId,
                        principalTable: "Productions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionId = table.Column<int>(type: "int", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionColors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionColors_Productions_ProductionId",
                        column: x => x.ProductionId,
                        principalTable: "Productions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyProductionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DailyTarget = table.Column<int>(type: "int", nullable: false),
                    HourlyTarget = table.Column<int>(type: "int", nullable: false),
                    H1 = table.Column<int>(type: "int", nullable: false),
                    H2 = table.Column<int>(type: "int", nullable: false),
                    H3 = table.Column<int>(type: "int", nullable: false),
                    H4 = table.Column<int>(type: "int", nullable: false),
                    H5 = table.Column<int>(type: "int", nullable: false),
                    H6 = table.Column<int>(type: "int", nullable: false),
                    H7 = table.Column<int>(type: "int", nullable: false),
                    H8 = table.Column<int>(type: "int", nullable: false),
                    H9 = table.Column<int>(type: "int", nullable: false),
                    H10 = table.Column<int>(type: "int", nullable: false),
                    H11 = table.Column<int>(type: "int", nullable: false),
                    H12 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyProductionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyProductionRecords_ProductionAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "ProductionAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionTargets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DailyTarget = table.Column<int>(type: "int", nullable: false),
                    HourlyTarget = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionTargets_ProductionAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "ProductionAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyProductionRecords_AssignmentId",
                table: "DailyProductionRecords",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionAssignments_LineId",
                table: "ProductionAssignments",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionAssignments_ProductionId",
                table: "ProductionAssignments",
                column: "ProductionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionColors_ProductionId",
                table: "ProductionColors",
                column: "ProductionId");

            migrationBuilder.CreateIndex(
                name: "IX_Productions_StyleNo",
                table: "Productions",
                column: "StyleNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTargets_AssignmentId",
                table: "ProductionTargets",
                column: "AssignmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyProductionRecords");

            migrationBuilder.DropTable(
                name: "ProductionColors");

            migrationBuilder.DropTable(
                name: "ProductionTargets");

            migrationBuilder.DropTable(
                name: "ProductionAssignments");

            migrationBuilder.DropTable(
                name: "ProductionLines");

            migrationBuilder.DropTable(
                name: "Productions");
        }
    }
}
