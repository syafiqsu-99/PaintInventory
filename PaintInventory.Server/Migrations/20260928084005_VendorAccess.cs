using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintInventory.Server.Migrations
{
    /// <inheritdoc />
    public partial class VendorAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaintInventory_StockTransactions_CounterpartyVendorId",
                table: "PaintInventory_StockTransactions");

            migrationBuilder.AddColumn<string>(
                name: "AccessCodeHash",
                table: "PaintInventory_Vendors",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccessCodeUpdatedAt",
                table: "PaintInventory_Vendors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "PaintInventory_StockTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedBy",
                table: "PaintInventory_StockTransactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedQty",
                table: "PaintInventory_StockTransactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_StockTransactions_CounterpartyVendorId_ReceivedAt",
                table: "PaintInventory_StockTransactions",
                columns: new[] { "CounterpartyVendorId", "ReceivedAt" });

            // Transfers recorded before this migration were credited instantly; mark them received.
            migrationBuilder.Sql(
                "UPDATE PaintInventory_StockTransactions " +
                "SET ReceivedAt = [Timestamp], ReceivedQty = Quantity, ReceivedBy = Operator " +
                "WHERE Direction = 3 AND ReceivedAt IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaintInventory_StockTransactions_CounterpartyVendorId_ReceivedAt",
                table: "PaintInventory_StockTransactions");

            migrationBuilder.DropColumn(
                name: "AccessCodeHash",
                table: "PaintInventory_Vendors");

            migrationBuilder.DropColumn(
                name: "AccessCodeUpdatedAt",
                table: "PaintInventory_Vendors");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "PaintInventory_StockTransactions");

            migrationBuilder.DropColumn(
                name: "ReceivedBy",
                table: "PaintInventory_StockTransactions");

            migrationBuilder.DropColumn(
                name: "ReceivedQty",
                table: "PaintInventory_StockTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_StockTransactions_CounterpartyVendorId",
                table: "PaintInventory_StockTransactions",
                column: "CounterpartyVendorId");
        }
    }
}
