using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintInventory.Server.Migrations
{
    /// <inheritdoc />
    public partial class PaintInventory3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Manufacturer",
                table: "PaintInventory_Products",
                newName: "TemperatureResistance");

            migrationBuilder.RenameColumn(
                name: "DefaultShade",
                table: "PaintInventory_Products",
                newName: "Technology");

            migrationBuilder.AddColumn<int>(
                name: "Brand",
                table: "PaintInventory_Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "PaintInventory_Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CleanerProductId",
                table: "PaintInventory_Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Colour",
                table: "PaintInventory_Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoverageMaxM2L",
                table: "PaintInventory_Products",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoverageMinM2L",
                table: "PaintInventory_Products",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DftMaxUm",
                table: "PaintInventory_Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DftMinUm",
                table: "PaintInventory_Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GlossLevel",
                table: "PaintInventory_Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MsdsUrl",
                table: "PaintInventory_Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PotLifeMinutes",
                table: "PaintInventory_Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductFamily",
                table: "PaintInventory_Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductType",
                table: "PaintInventory_Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShelfLifeMonths",
                table: "PaintInventory_Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "PaintInventory_Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TdsUrl",
                table: "PaintInventory_Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThinnerProductId",
                table: "PaintInventory_Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VocGramsPerLitre",
                table: "PaintInventory_Products",
                type: "decimal(7,2)",
                precision: 7,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VolumeSolidsPct",
                table: "PaintInventory_Products",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WftMaxUm",
                table: "PaintInventory_Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WftMinUm",
                table: "PaintInventory_Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_Products_Brand",
                table: "PaintInventory_Products",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_Products_CleanerProductId",
                table: "PaintInventory_Products",
                column: "CleanerProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_Products_ProductType",
                table: "PaintInventory_Products",
                column: "ProductType");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_Products_ThinnerProductId",
                table: "PaintInventory_Products",
                column: "ThinnerProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaintInventory_Products_PaintInventory_Products_CleanerProductId",
                table: "PaintInventory_Products",
                column: "CleanerProductId",
                principalTable: "PaintInventory_Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaintInventory_Products_PaintInventory_Products_ThinnerProductId",
                table: "PaintInventory_Products",
                column: "ThinnerProductId",
                principalTable: "PaintInventory_Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaintInventory_Products_PaintInventory_Products_CleanerProductId",
                table: "PaintInventory_Products");

            migrationBuilder.DropForeignKey(
                name: "FK_PaintInventory_Products_PaintInventory_Products_ThinnerProductId",
                table: "PaintInventory_Products");

            migrationBuilder.DropIndex(
                name: "IX_PaintInventory_Products_Brand",
                table: "PaintInventory_Products");

            migrationBuilder.DropIndex(
                name: "IX_PaintInventory_Products_CleanerProductId",
                table: "PaintInventory_Products");

            migrationBuilder.DropIndex(
                name: "IX_PaintInventory_Products_ProductType",
                table: "PaintInventory_Products");

            migrationBuilder.DropIndex(
                name: "IX_PaintInventory_Products_ThinnerProductId",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "CleanerProductId",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "Colour",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "CoverageMaxM2L",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "CoverageMinM2L",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "DftMaxUm",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "DftMinUm",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "GlossLevel",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "MsdsUrl",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "PotLifeMinutes",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "ProductFamily",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "ShelfLifeMonths",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "TdsUrl",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "ThinnerProductId",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "VocGramsPerLitre",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "VolumeSolidsPct",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "WftMaxUm",
                table: "PaintInventory_Products");

            migrationBuilder.DropColumn(
                name: "WftMinUm",
                table: "PaintInventory_Products");

            migrationBuilder.RenameColumn(
                name: "TemperatureResistance",
                table: "PaintInventory_Products",
                newName: "Manufacturer");

            migrationBuilder.RenameColumn(
                name: "Technology",
                table: "PaintInventory_Products",
                newName: "DefaultShade");
        }
    }
}
