using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaintInventory.Server.Migrations
{
    /// <inheritdoc />
    public partial class PaintInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaintInventory_AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Entity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintInventory_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaintInventory_Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Gtin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Component = table.Column<int>(type: "int", nullable: false),
                    PackVolume = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultShade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MixRatio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartnerProductId = table.Column<int>(type: "int", nullable: true),
                    UnNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HazardFlags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TracksExpiry = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintInventory_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaintInventory_Products_PaintInventory_Products_PartnerProductId",
                        column: x => x.PartnerProductId,
                        principalTable: "PaintInventory_Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaintInventory_Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ipo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Customer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreparedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreparedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintInventory_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaintInventory_Vendors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsOwnCompany = table.Column<bool>(type: "bit", nullable: false),
                    StoresStock = table.Column<bool>(type: "bit", nullable: false),
                    DoesBlasting = table.Column<bool>(type: "bit", nullable: false),
                    DoesPainting = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintInventory_Vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaintInventory_ReportItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaintReportId = table.Column<int>(type: "int", nullable: false),
                    ItemNo = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaintingSpec = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComponentDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComponentLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AbrasiveBlasting = table.Column<bool>(type: "bit", nullable: false),
                    RequiredTotalDftUm = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    MeasuredTotalDftUm = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    AdhesionTestPerformed = table.Column<bool>(type: "bit", nullable: true),
                    AdhesionTestType = table.Column<int>(type: "int", nullable: false),
                    MekTestNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BlastVendorId = table.Column<int>(type: "int", nullable: true),
                    PaintingVendorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintInventory_ReportItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaintInventory_ReportItems_PaintInventory_Reports_PaintReportId",
                        column: x => x.PaintReportId,
                        principalTable: "PaintInventory_Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaintInventory_ReportItems_PaintInventory_Vendors_BlastVendorId",
                        column: x => x.BlastVendorId,
                        principalTable: "PaintInventory_Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaintInventory_ReportItems_PaintInventory_Vendors_PaintingVendorId",
                        column: x => x.PaintingVendorId,
                        principalTable: "PaintInventory_Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaintInventory_StockBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaintProductId = table.Column<int>(type: "int", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    OnHandQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReorderLevel = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintInventory_StockBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaintInventory_StockBalances_PaintInventory_Products_PaintProductId",
                        column: x => x.PaintProductId,
                        principalTable: "PaintInventory_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaintInventory_StockBalances_PaintInventory_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "PaintInventory_Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaintInventory_CoatLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaintReportItemId = table.Column<int>(type: "int", nullable: false),
                    CoatType = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    PartAProductId = table.Column<int>(type: "int", nullable: true),
                    PartBProductId = table.Column<int>(type: "int", nullable: true),
                    PaintIdText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartABatch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartBBatch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Shade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredThicknessUm = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    MeasuredThicknessUm = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    HumidityPct = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    AirTempC = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    SubstrateTempC = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    DewPointC = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeductFromStock = table.Column<bool>(type: "bit", nullable: false),
                    StockLocationVendorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintInventory_CoatLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaintInventory_CoatLines_PaintInventory_Products_PartAProductId",
                        column: x => x.PartAProductId,
                        principalTable: "PaintInventory_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaintInventory_CoatLines_PaintInventory_Products_PartBProductId",
                        column: x => x.PartBProductId,
                        principalTable: "PaintInventory_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaintInventory_CoatLines_PaintInventory_ReportItems_PaintReportItemId",
                        column: x => x.PaintReportItemId,
                        principalTable: "PaintInventory_ReportItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaintInventory_CoatLines_PaintInventory_Vendors_StockLocationVendorId",
                        column: x => x.StockLocationVendorId,
                        principalTable: "PaintInventory_Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaintInventory_SurfacePreps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaintReportItemId = table.Column<int>(type: "int", nullable: false),
                    GradeOfCleanliness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredRoughness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeasuredRoughness = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    HumidityPct = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    AirTempC = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    SubstrateTempC = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    DewPointC = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintInventory_SurfacePreps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaintInventory_SurfacePreps_PaintInventory_ReportItems_PaintReportItemId",
                        column: x => x.PaintReportItemId,
                        principalTable: "PaintInventory_ReportItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaintInventory_StockTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaintProductId = table.Column<int>(type: "int", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    CounterpartyVendorId = table.Column<int>(type: "int", nullable: true),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Batch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Shade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PackVolume = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ManufacturingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BestBefore = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoatLineId = table.Column<int>(type: "int", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaintInventory_StockTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaintInventory_StockTransactions_PaintInventory_CoatLines_CoatLineId",
                        column: x => x.CoatLineId,
                        principalTable: "PaintInventory_CoatLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaintInventory_StockTransactions_PaintInventory_Products_PaintProductId",
                        column: x => x.PaintProductId,
                        principalTable: "PaintInventory_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaintInventory_StockTransactions_PaintInventory_Vendors_CounterpartyVendorId",
                        column: x => x.CounterpartyVendorId,
                        principalTable: "PaintInventory_Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaintInventory_StockTransactions_PaintInventory_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "PaintInventory_Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_CoatLines_PaintReportItemId_Sequence",
                table: "PaintInventory_CoatLines",
                columns: new[] { "PaintReportItemId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_CoatLines_PartAProductId",
                table: "PaintInventory_CoatLines",
                column: "PartAProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_CoatLines_PartBProductId",
                table: "PaintInventory_CoatLines",
                column: "PartBProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_CoatLines_StockLocationVendorId",
                table: "PaintInventory_CoatLines",
                column: "StockLocationVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_Products_Gtin",
                table: "PaintInventory_Products",
                column: "Gtin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_Products_PartnerProductId",
                table: "PaintInventory_Products",
                column: "PartnerProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_ReportItems_BlastVendorId",
                table: "PaintInventory_ReportItems",
                column: "BlastVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_ReportItems_PaintingVendorId",
                table: "PaintInventory_ReportItems",
                column: "PaintingVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_ReportItems_PaintReportId_ItemNo",
                table: "PaintInventory_ReportItems",
                columns: new[] { "PaintReportId", "ItemNo" });

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_Reports_Ipo",
                table: "PaintInventory_Reports",
                column: "Ipo");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_StockBalances_PaintProductId_VendorId",
                table: "PaintInventory_StockBalances",
                columns: new[] { "PaintProductId", "VendorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_StockBalances_VendorId",
                table: "PaintInventory_StockBalances",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_StockTransactions_CoatLineId",
                table: "PaintInventory_StockTransactions",
                column: "CoatLineId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_StockTransactions_CounterpartyVendorId",
                table: "PaintInventory_StockTransactions",
                column: "CounterpartyVendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_StockTransactions_PaintProductId",
                table: "PaintInventory_StockTransactions",
                column: "PaintProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_StockTransactions_Timestamp",
                table: "PaintInventory_StockTransactions",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_StockTransactions_VendorId",
                table: "PaintInventory_StockTransactions",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_SurfacePreps_PaintReportItemId",
                table: "PaintInventory_SurfacePreps",
                column: "PaintReportItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaintInventory_Vendors_Name",
                table: "PaintInventory_Vendors",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaintInventory_AuditLogs");

            migrationBuilder.DropTable(
                name: "PaintInventory_StockBalances");

            migrationBuilder.DropTable(
                name: "PaintInventory_StockTransactions");

            migrationBuilder.DropTable(
                name: "PaintInventory_SurfacePreps");

            migrationBuilder.DropTable(
                name: "PaintInventory_CoatLines");

            migrationBuilder.DropTable(
                name: "PaintInventory_Products");

            migrationBuilder.DropTable(
                name: "PaintInventory_ReportItems");

            migrationBuilder.DropTable(
                name: "PaintInventory_Reports");

            migrationBuilder.DropTable(
                name: "PaintInventory_Vendors");
        }
    }
}
