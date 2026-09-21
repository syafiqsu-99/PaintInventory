using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InventoryController(PaintInventoryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryLevelDto>>> GetLevels(
        [FromQuery] int? vendorId, CancellationToken ct = default)
    {
        var rows = await BaseLevels(vendorId).OrderBy(l => l.ProductName).ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<InventoryLevelDto>>> GetLowStock(
        [FromQuery] int? vendorId, CancellationToken ct = default)
    {
        var rows = await BaseLevels(vendorId, onlyLow: true).OrderBy(l => l.OnHandQty).ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet("{productId:int}/history")]
    public async Task<ActionResult<IEnumerable<StockHistoryDto>>> GetHistory(
        int productId, [FromQuery] int? vendorId, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        if (!await db.PaintProducts.AnyAsync(p => p.Id == productId, ct))
            return NotFound(new { error = $"Product {productId} not found." });

        var q = db.StockTransactions.AsNoTracking().Where(t => t.PaintProductId == productId);
        if (vendorId is not null)
            q = q.Where(t => t.VendorId == vendorId || t.CounterpartyVendorId == vendorId);

        var rows = await q
            .OrderByDescending(t => t.Timestamp)
            .Take(Math.Clamp(take, 1, 500))
            .Select(t => new StockHistoryDto(
                t.Id, t.Direction, t.Quantity, t.Batch, t.Vendor.Name,
                t.CounterpartyVendor != null ? t.CounterpartyVendor.Name : null,
                t.Operator, t.Notes, t.Timestamp))
            .ToListAsync(ct);

        return Ok(rows);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken ct)
    {
        var totalProducts = await db.PaintProducts.CountAsync(p => p.IsActive, ct);
        var lowStockCount = await db.StockBalances
            .CountAsync(b => b.ReorderLevel != null && b.OnHandQty <= b.ReorderLevel, ct);
        var totalOnHand = await db.StockBalances.SumAsync(b => (decimal?)b.OnHandQty, ct) ?? 0m;

        var cutoff = DateTime.UtcNow.Date.AddDays(-13);
        var usage = await db.StockTransactions.AsNoTracking()
            .Where(t => t.Direction == StockDirection.Out && t.Timestamp >= cutoff)
            .GroupBy(t => t.Timestamp.Date)
            .Select(g => new UsagePointDto(g.Key, g.Sum(x => x.Quantity)))
            .OrderBy(u => u.Date)
            .ToListAsync(ct);

        return Ok(new DashboardDto(totalProducts, lowStockCount, totalOnHand, usage));
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> SetReorder(SetReorderRequest req, CancellationToken ct)
    {
        if (!await db.PaintProducts.AnyAsync(p => p.Id == req.ProductId, ct))
            return NotFound(new { error = $"Product {req.ProductId} not found." });
        if (!await db.Vendors.AnyAsync(v => v.Id == req.VendorId, ct))
            return NotFound(new { error = $"Location {req.VendorId} not found." });

        var balance = await db.StockBalances
            .FirstOrDefaultAsync(b => b.PaintProductId == req.ProductId && b.VendorId == req.VendorId, ct);
        if (balance is null)
        {
            balance = new StockBalance
            {
                PaintProductId = req.ProductId,
                VendorId = req.VendorId,
                OnHandQty = 0m
            };
            db.StockBalances.Add(balance);
        }

        balance.ReorderLevel = req.ReorderLevel;
        balance.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(new { req.ProductId, req.VendorId, req.ReorderLevel });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] int? vendorId, CancellationToken ct = default)
    {
        var rows = await BaseLevels(vendorId).OrderBy(l => l.ProductName).ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Stock");

        string[] headers =
            ["GTIN", "Product", "Component", "Shade", "Location", "On Hand", "Unit", "Reorder Level", "Low Stock", "Updated (UTC)"];
        for (var c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];
        ws.Row(1).Style.Font.Bold = true;

        var r = 2;
        foreach (var i in rows)
        {
            ws.Cell(r, 1).Value = i.Gtin;
            ws.Cell(r, 2).Value = i.ProductName;
            ws.Cell(r, 3).Value = i.Component.ToString();
            ws.Cell(r, 4).Value = i.Shade ?? string.Empty;
            ws.Cell(r, 5).Value = i.VendorName;
            ws.Cell(r, 6).Value = i.OnHandQty;
            ws.Cell(r, 7).Value = i.Unit ?? string.Empty;
            if (i.ReorderLevel.HasValue) ws.Cell(r, 8).Value = i.ReorderLevel.Value;
            ws.Cell(r, 9).Value = i.IsLowStock ? "Yes" : "No";
            if (i.UpdatedAt.HasValue) ws.Cell(r, 10).Value = i.UpdatedAt.Value;
            r++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var fileName = $"paint-stock-{DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private IQueryable<InventoryLevelDto> BaseLevels(int? vendorId, bool onlyLow = false)
    {
        var q = db.StockBalances.AsNoTracking().Where(b => b.PaintProduct.IsActive);
        if (vendorId is not null) q = q.Where(b => b.VendorId == vendorId);
        if (onlyLow) q = q.Where(b => b.ReorderLevel != null && b.OnHandQty <= b.ReorderLevel);

        return q.Select(b => new InventoryLevelDto(
            b.Id, b.PaintProductId, b.PaintProduct.Gtin, b.PaintProduct.ProductName, b.PaintProduct.Component,
            b.PaintProduct.DefaultShade ?? b.PaintProduct.RalCode, b.PaintProduct.Unit,
            b.VendorId, b.Vendor.Name, b.OnHandQty, b.ReorderLevel,
            b.ReorderLevel != null && b.OnHandQty <= b.ReorderLevel, b.UpdatedAt));
    }
}