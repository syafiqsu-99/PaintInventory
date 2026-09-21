using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintInventory.Server.Migrations
{
    /// <inheritdoc />
    public partial class PaintInventory2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PartAQtyUsed",
                table: "PaintInventory_CoatLines",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PartBQtyUsed",
                table: "PaintInventory_CoatLines",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartAQtyUsed",
                table: "PaintInventory_CoatLines");

            migrationBuilder.DropColumn(
                name: "PartBQtyUsed",
                table: "PaintInventory_CoatLines");
        }
    }
}
