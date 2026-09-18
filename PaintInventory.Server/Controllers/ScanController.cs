using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScanController : ControllerBase
{
    private readonly PaintInventoryDbContext _db;

    public ScanController(PaintInventoryDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> RecordScan([FromBody] ScanRecord dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.BarcodeScanned))
            return BadRequest("Invalid scan payload");

        // Try to find referenced paint item
        var paint = await _db.PaintItems.FirstOrDefaultAsync(p => p.Barcode == dto.BarcodeScanned);
        if (paint != null)
        {
            dto.PaintItemId = paint.Id;
        }

        dto.Timestamp = DateTime.UtcNow;
        _db.ScanRecords.Add(dto);

        // If the action implies movement to a location, record location history
        if (!string.IsNullOrWhiteSpace(dto.Location) && dto.Quantity != 0)
        {
            var loc = new LocationHistory
            {
                PaintItemId = dto.PaintItemId ?? 0,
                Location = dto.Location,
                QuantityMoved = dto.Quantity,
                Timestamp = dto.Timestamp,
                Notes = dto.Notes
            };

            // Only add if we have a PaintItemId
            if (dto.PaintItemId != null)
                _db.LocationHistories.Add(loc);
        }

        // Add simple audit log
        var audit = new AuditLog
        {
            Entity = "ScanRecord",
            EntityId = dto.Id.ToString(),
            Action = dto.Action,
            ChangedAt = DateTime.UtcNow,
            Details = System.Text.Json.JsonSerializer.Serialize(dto)
        };
        _db.AuditLogs.Add(audit);

        await _db.SaveChangesAsync();

        return Ok(dto);
    }
}
