using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ScanController(PaintInventoryDbContext db) : ControllerBase
{
    private static readonly string[] AllowedActions = ["Use", "Receive", "Adjust"];

    [HttpPost]
    public async Task<ActionResult<ScanResult>> RecordScan(ScanRequest req, CancellationToken ct)
    {
        var action = req.Action?.Trim();
        if (action is null || !AllowedActions.Contains(action))
            return BadRequest(new { error = "Action must be Use, Receive, or Adjust." });
        if (req.Quantity < 0)
            return BadRequest(new { error = "Quantity cannot be negative." });

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var item = await db.PaintItems.FirstOrDefaultAsync(p => p.Barcode == req.Barcode, ct);
        if (item is null)
            return NotFound(new { error = $"No paint item registered for barcode '{req.Barcode}'. Register it first." });

        var delta = action switch
        {
            "Receive" => req.Quantity,
            "Use" => -req.Quantity,
            "Adjust" => req.Quantity - item.OnHand,
            _ => 0m
        };

        var now = DateTime.UtcNow;
        item.OnHand += delta;
        item.UpdatedAt = now;

        var scan = new ScanRecord
        {
            PaintItemId = item.Id,
            BarcodeScanned = req.Barcode,
            Action = action,
            Quantity = req.Quantity,
            Unit = req.Unit ?? item.Unit,
            DeviceId = req.DeviceId,
            Operator = req.Operator,
            Location = req.Location,
            Notes = req.Notes,
            Timestamp = now
        };
        db.ScanRecords.Add(scan);

        if (!string.IsNullOrWhiteSpace(req.Location) && delta != 0)
        {
            db.LocationHistories.Add(new LocationHistory
            {
                PaintItemId = item.Id,
                Location = req.Location!,
                QuantityMoved = delta,
                Notes = req.Notes,
                Timestamp = now
            });
        }

        db.AuditLogs.Add(new AuditLog
        {
            Entity = nameof(ScanRecord),
            Action = action,
            ChangedBy = req.Operator,
            ChangedAt = now,
            Details = JsonSerializer.Serialize(new
            {
                req.Barcode,
                action,
                req.Quantity,
                delta,
                newOnHand = item.OnHand
            })
        });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var isLow = item.ReorderLevel.HasValue && item.OnHand <= item.ReorderLevel.Value;
        return Ok(new ScanResult(scan.Id, item.Id, item.Barcode, action, delta, item.OnHand, isLow, now));
    }
}