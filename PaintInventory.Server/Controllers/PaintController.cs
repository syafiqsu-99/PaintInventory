using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;
using QRCoder;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PaintController(PaintInventoryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaintItemDto>>> GetAll(CancellationToken ct)
    {
        var items = await db.PaintItems
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new PaintItemDto(
                p.Id, p.Barcode, p.SKU, p.Name, p.ColorCode, p.Volume, p.Unit, p.Batch,
                p.Manufacturer, p.OnHand, p.ReorderLevel,
                p.ReorderLevel.HasValue && p.OnHand <= p.ReorderLevel.Value,
                p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("{barcode}")]
    public async Task<ActionResult<PaintItemDto>> GetByBarcode(string barcode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return BadRequest(new { error = "Barcode is required." });

        var item = await db.PaintItems
            .AsNoTracking()
            .Where(p => p.Barcode == barcode)
            .Select(p => new PaintItemDto(
                p.Id, p.Barcode, p.SKU, p.Name, p.ColorCode, p.Volume, p.Unit, p.Batch,
                p.Manufacturer, p.OnHand, p.ReorderLevel,
                p.ReorderLevel.HasValue && p.OnHand <= p.ReorderLevel.Value,
                p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(ct);

        return item is null
            ? NotFound(new { error = $"No paint item found for barcode '{barcode}'." })
            : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<PaintItemDto>> Create(PaintItemRequest req, CancellationToken ct)
    {
        if (await db.PaintItems.AnyAsync(p => p.Barcode == req.Barcode, ct))
            return Conflict(new { error = $"Barcode '{req.Barcode}' is already registered." });

        var item = new PaintItem
        {
            Barcode = req.Barcode,
            SKU = req.Sku,
            Name = req.Name,
            ColorCode = req.ColorCode,
            Volume = req.Volume,
            Unit = req.Unit,
            Batch = req.Batch,
            Manufacturer = req.Manufacturer,
            ReorderLevel = req.ReorderLevel,
            CreatedAt = DateTime.UtcNow
        };

        db.PaintItems.Add(item);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetByBarcode), new { barcode = item.Barcode }, ToDto(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PaintItemDto>> Update(int id, PaintItemRequest req, CancellationToken ct)
    {
        var item = await db.PaintItems.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (item is null)
            return NotFound(new { error = $"Paint item {id} not found." });

        if (item.Barcode != req.Barcode &&
            await db.PaintItems.AnyAsync(p => p.Barcode == req.Barcode && p.Id != id, ct))
            return Conflict(new { error = $"Barcode '{req.Barcode}' is already registered." });

        item.Barcode = req.Barcode;
        item.SKU = req.Sku;
        item.Name = req.Name;
        item.ColorCode = req.ColorCode;
        item.Volume = req.Volume;
        item.Unit = req.Unit;
        item.Batch = req.Batch;
        item.Manufacturer = req.Manufacturer;
        item.ReorderLevel = req.ReorderLevel;
        item.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Ok(ToDto(item));
    }

    [HttpGet("{barcode}/qr")]
    public async Task<IActionResult> GetQr(string barcode, CancellationToken ct)
    {
        var exists = await db.PaintItems.AnyAsync(p => p.Barcode == barcode, ct);
        if (!exists)
            return NotFound(new { error = $"No paint item found for barcode '{barcode}'." });

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(barcode, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data).GetGraphic(20);

        return File(png, "image/png");
    }

    private static PaintItemDto ToDto(PaintItem p) => new(
        p.Id, p.Barcode, p.SKU, p.Name, p.ColorCode, p.Volume, p.Unit, p.Batch,
        p.Manufacturer, p.OnHand, p.ReorderLevel,
        p.ReorderLevel.HasValue && p.OnHand <= p.ReorderLevel.Value,
        p.CreatedAt, p.UpdatedAt);
}