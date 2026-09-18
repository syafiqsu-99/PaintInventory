using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaintController : ControllerBase
{
    private readonly PaintInventoryDbContext _db;

    public PaintController(PaintInventoryDbContext db)
    {
        _db = db;
    }

    [HttpGet("{barcode}")]
    public async Task<IActionResult> GetByBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return BadRequest("barcode required");

        var paint = await _db.PaintItems
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Barcode == barcode);

        if (paint == null)
            return NotFound();

        return Ok(paint);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PaintItem item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.Barcode))
            return BadRequest();

        item.CreatedAt = DateTime.UtcNow;
        _db.PaintItems.Add(item);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByBarcode), new { barcode = item.Barcode }, item);
    }
}
