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
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetInventory(CancellationToken ct)
    {
        var items = await db.PaintItems
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new InventoryItemDto(
                p.Id, p.Barcode, p.Name, p.ColorCode, p.Unit, p.OnHand, p.ReorderLevel,
                p.ReorderLevel.HasValue && p.OnHand <= p.ReorderLevel.Value, p.UpdatedAt))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetLowStock(CancellationToken ct)
    {
        var items = await db.PaintItems
            .AsNoTracking()
            .Where(p => p.ReorderLevel.HasValue && p.OnHand <= p.ReorderLevel.Value)
            .OrderBy(p => p.OnHand)
            .Select(p => new InventoryItemDto(
                p.Id, p.Barcode, p.Name, p.ColorCode, p.Unit, p.OnHand, p.ReorderLevel,
                true, p.UpdatedAt))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<IEnumerable<ScanHistoryDto>>> GetHistory(
        int id, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var exists = await db.PaintItems.AnyAsync(p => p.Id == id, ct);
        if (!exists)
            return NotFound(new { error = $"Paint item {id} not found." });

        var history = await db.ScanRecords
            .AsNoTracking()
            .Where(s => s.PaintItemId == id)
            .OrderByDescending(s => s.Timestamp)
            .Take(Math.Clamp(take, 1, 500))
            .Select(s => new ScanHistoryDto(
                s.Id, s.Action, s.Quantity, s.Unit, s.Location, s.Operator, s.Notes, s.Timestamp))
            .ToListAsync(ct);

        return Ok(history);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken ct)
    {
        var totalItems = await db.PaintItems.CountAsync(ct);
        var lowStockCount = await db.PaintItems
            .CountAsync(p => p.ReorderLevel.HasValue && p.OnHand <= p.ReorderLevel.Value, ct);
        var totalOnHand = await db.PaintItems.SumAsync(p => p.OnHand, ct);

        var cutoff = DateTime.UtcNow.Date.AddDays(-13);
        var usage = await db.ScanRecords
            .AsNoTracking()
            .Where(s => s.Action == "Use" && s.Timestamp >= cutoff)
            .GroupBy(s => s.Timestamp.Date)
            .Select(g => new UsagePointDto(g.Key, g.Sum(x => x.Quantity)))
            .OrderBy(u => u.Date)
            .ToListAsync(ct);

        return Ok(new DashboardDto(totalItems, lowStockCount, totalOnHand, usage));
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var items = await db.PaintItems
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Barcode,
                p.Name,
                p.SKU,
                p.ColorCode,
                p.Unit,
                p.OnHand,
                p.ReorderLevel,
                IsLow = p.ReorderLevel.HasValue && p.OnHand <= p.ReorderLevel.Value,
                p.UpdatedAt
            })
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Stock");

        string[] headers = ["Barcode", "Name", "SKU", "Color", "Unit", "On Hand", "Reorder Level", "Low Stock", "Updated (UTC)"];
        for (var c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];
        ws.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var i in items)
        {
            ws.Cell(row, 1).Value = i.Barcode;
            ws.Cell(row, 2).Value = i.Name ?? string.Empty;
            ws.Cell(row, 3).Value = i.SKU ?? string.Empty;
            ws.Cell(row, 4).Value = i.ColorCode ?? string.Empty;
            ws.Cell(row, 5).Value = i.Unit ?? string.Empty;
            ws.Cell(row, 6).Value = i.OnHand;
            if (i.ReorderLevel.HasValue) ws.Cell(row, 7).Value = i.ReorderLevel.Value;
            ws.Cell(row, 8).Value = i.IsLow ? "Yes" : "No";
            if (i.UpdatedAt.HasValue) ws.Cell(row, 9).Value = i.UpdatedAt.Value;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var fileName = $"paint-stock-{DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}