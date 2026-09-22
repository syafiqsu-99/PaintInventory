using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController(PaintInventoryDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
        [FromQuery] int expiryDays = 60, CancellationToken ct = default)
    {
        var window = Math.Clamp(expiryDays, 1, 365);
        var today = DateTime.UtcNow.Date;

        var totalProducts = await db.PaintProducts.CountAsync(p => p.IsActive, ct);
        var totalLocations = await db.Vendors.CountAsync(v => v.IsActive && v.StoresStock, ct);
        var totalOnHand = await db.StockBalances.SumAsync(b => (decimal?)b.OnHandQty, ct) ?? 0m;

        var lowStockCount = await db.StockBalances
            .CountAsync(b => b.PaintProduct.IsActive && b.ReorderLevel != null && b.OnHandQty <= b.ReorderLevel, ct);

        var outOfStockCount = await db.PaintProducts
            .CountAsync(p => p.IsActive && !db.StockBalances.Any(b => b.PaintProductId == p.Id && b.OnHandQty > 0), ct);

        var usageCutoff = today.AddDays(-13);
        var usageGrouped = await db.StockTransactions.AsNoTracking()
            .Where(t => t.Direction == StockDirection.Out && t.Timestamp >= usageCutoff)
            .GroupBy(t => t.Timestamp.Date)
            .Select(g => new { Date = g.Key, Qty = g.Sum(x => x.Quantity) })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
        var usage = usageGrouped.Select(x => new UsagePointDto(x.Date, x.Qty)).ToList();

        var stockByLocationRaw = await db.StockBalances.AsNoTracking()
            .Where(b => b.PaintProduct.IsActive && b.OnHandQty > 0)
            .GroupBy(b => b.Vendor.Name)
            .Select(g => new { Name = g.Key, OnHand = g.Sum(x => x.OnHandQty) })
            .OrderByDescending(x => x.OnHand)
            .ToListAsync(ct);
        var stockByLocation = stockByLocationRaw
            .Select(x => new LocationStockDto(x.Name, x.OnHand))
            .ToList();

        var topUsedCutoff = today.AddDays(-30);
        var topUsedRaw = await db.StockTransactions.AsNoTracking()
            .Where(t => t.Direction == StockDirection.Out && t.Timestamp >= topUsedCutoff)
            .GroupBy(t => t.PaintProduct.ProductName)
            .Select(g => new { Name = g.Key, Qty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Qty)
            .Take(8)
            .ToListAsync(ct);
        var topUsed = topUsedRaw
            .Select(x => new TopUsedDto(x.Name, x.Qty))
            .ToList();

        var expiryCutoff = today.AddDays(window);
        var expiringRaw = await db.StockTransactions.AsNoTracking()
            .Where(t => t.Direction == StockDirection.In
                        && t.BestBefore != null
                        && t.BestBefore <= expiryCutoff
                        && db.StockBalances.Any(b =>
                            b.PaintProductId == t.PaintProductId && b.VendorId == t.VendorId && b.OnHandQty > 0))
            .OrderBy(t => t.BestBefore)
            .Select(t => new
            {
                t.PaintProductId,
                t.PaintProduct.Gtin,
                t.PaintProduct.ProductName,
                VendorName = t.Vendor.Name,
                t.Batch,
                BestBefore = t.BestBefore!.Value,
                t.Quantity
            })
            .Take(100)
            .ToListAsync(ct);

        var expiring = expiringRaw
            .Select(t => new ExpiringLotDto(
                t.PaintProductId, t.Gtin, t.ProductName, t.VendorName, t.Batch,
                t.BestBefore, (int)(t.BestBefore.Date - today).TotalDays, t.Quantity))
            .ToList();

        var lowStock = await db.StockBalances.AsNoTracking()
            .Where(b => b.PaintProduct.IsActive && b.ReorderLevel != null && b.OnHandQty <= b.ReorderLevel)
            .OrderBy(b => b.OnHandQty)
            .Select(b => new InventoryLevelDto(
                b.Id, b.PaintProductId, b.PaintProduct.Gtin, b.PaintProduct.ProductName, b.PaintProduct.Component,
                b.PaintProduct.DefaultShade ?? b.PaintProduct.RalCode, b.PaintProduct.Unit,
                b.VendorId, b.Vendor.Name, b.OnHandQty, b.ReorderLevel, true, b.UpdatedAt))
            .Take(50)
            .ToListAsync(ct);

        return Ok(new DashboardSummaryDto(
            totalProducts, totalLocations, lowStockCount, outOfStockCount, expiring.Count,
            totalOnHand, window, usage, stockByLocation, topUsed, expiring, lowStock));
    }
}