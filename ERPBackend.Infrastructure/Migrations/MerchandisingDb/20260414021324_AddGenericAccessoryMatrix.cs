using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPBackend.Infrastructure.Migrations.MerchandisingDb
{
    /// <inheritdoc />
    public partial class AddGenericAccessoryMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgramAccessoryRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramOrderId = table.Column<int>(type: "int", nullable: false),
                    ProgramSizeBreakdownId = table.Column<int>(type: "int", nullable: false),
                    AccessoryType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MasterColorId = table.Column<int>(type: "int", nullable: true),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramAccessoryRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramAccessoryRequirements_FabricColorPantones_MasterColorId",
                        column: x => x.MasterColorId,
                        principalTable: "FabricColorPantones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProgramAccessoryRequirements_ProgramSizeBreakdowns_ProgramSizeBreakdownId",
                        column: x => x.ProgramSizeBreakdownId,
                        principalTable: "ProgramSizeBreakdowns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramAccessoryRequirements_MasterColorId",
                table: "ProgramAccessoryRequirements",
                column: "MasterColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramAccessoryRequirements_ProgramSizeBreakdownId",
                table: "ProgramAccessoryRequirements",
                column: "ProgramSizeBreakdownId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgramAccessoryRequirements");
        }
    }
}
