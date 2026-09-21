using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(PaintInventoryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
        [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var q = db.PaintProducts.AsNoTracking();
        if (!includeInactive) q = q.Where(p => p.IsActive);
        var items = await Project(q.OrderBy(p => p.ProductName)).ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{gtin}")]
    public async Task<ActionResult<ProductDto>> GetByGtin(string gtin, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(gtin))
            return BadRequest(new { error = "GTIN is required." });

        var item = await Project(db.PaintProducts.AsNoTracking().Where(p => p.Gtin == gtin)).FirstOrDefaultAsync(ct);
        return item is null
            ? NotFound(new { error = $"No product registered for GTIN '{gtin}'." })
            : Ok(item);
    }

    [HttpGet("by-id/{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
    {
        var item = await Project(db.PaintProducts.AsNoTracking().Where(p => p.Id == id)).FirstOrDefaultAsync(ct);
        return item is null ? NotFound(new { error = $"Product {id} not found." }) : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductRequest req, CancellationToken ct)
    {
        if (await db.PaintProducts.AnyAsync(p => p.Gtin == req.Gtin, ct))
            return Conflict(new { error = $"GTIN '{req.Gtin}' is already registered." });

        var product = new PaintProduct
        {
            Gtin = req.Gtin,
            ItemCode = req.ItemCode,
            ProductName = req.ProductName,
            Description = req.Description,
            Component = req.Component,
            PackVolume = req.PackVolume,
            Unit = req.Unit,
            DefaultShade = req.DefaultShade,
            RalCode = req.RalCode,
            Manufacturer = req.Manufacturer,
            MixRatio = req.MixRatio,
            PartnerProductId = req.PartnerProductId,
            UnNumber = req.UnNumber,
            HazardFlags = req.HazardFlags,
            TracksExpiry = req.TracksExpiry,
            CreatedAt = DateTime.UtcNow
        };

        db.PaintProducts.Add(product);
        await db.SaveChangesAsync(ct);

        if (req.PartnerProductId is int partnerId)
            await LinkPartnerAsync(product.Id, partnerId, ct);

        var dto = await Project(db.PaintProducts.AsNoTracking().Where(p => p.Id == product.Id)).FirstAsync(ct);
        return CreatedAtAction(nameof(GetByGtin), new { gtin = product.Gtin }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductRequest req, CancellationToken ct)
    {
        var product = await db.PaintProducts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return NotFound(new { error = $"Product {id} not found." });

        if (product.Gtin != req.Gtin && await db.PaintProducts.AnyAsync(p => p.Gtin == req.Gtin && p.Id != id, ct))
            return Conflict(new { error = $"GTIN '{req.Gtin}' is already registered." });

        product.Gtin = req.Gtin;
        product.ItemCode = req.ItemCode;
        product.ProductName = req.ProductName;
        product.Description = req.Description;
        product.Component = req.Component;
        product.PackVolume = req.PackVolume;
        product.Unit = req.Unit;
        product.DefaultShade = req.DefaultShade;
        product.RalCode = req.RalCode;
        product.Manufacturer = req.Manufacturer;
        product.MixRatio = req.MixRatio;
        product.PartnerProductId = req.PartnerProductId;
        product.UnNumber = req.UnNumber;
        product.HazardFlags = req.HazardFlags;
        product.TracksExpiry = req.TracksExpiry;
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        if (req.PartnerProductId is int partnerId)
            await LinkPartnerAsync(product.Id, partnerId, ct);

        var dto = await Project(db.PaintProducts.AsNoTracking().Where(p => p.Id == id)).FirstAsync(ct);
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var product = await db.PaintProducts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return NotFound(new { error = $"Product {id} not found." });

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task LinkPartnerAsync(int productId, int partnerId, CancellationToken ct)
    {
        var partner = await db.PaintProducts.FirstOrDefaultAsync(p => p.Id == partnerId, ct);
        if (partner is not null && partner.PartnerProductId != productId)
        {
            partner.PartnerProductId = productId;
            await db.SaveChangesAsync(ct);
        }
    }

    private static IQueryable<ProductDto> Project(IQueryable<PaintProduct> q) =>
        q.Select(p => new ProductDto(
            p.Id, p.Gtin, p.ItemCode, p.ProductName, p.Description, p.Component,
            p.PackVolume, p.Unit, p.DefaultShade, p.RalCode, p.Manufacturer, p.MixRatio,
            p.PartnerProductId, p.PartnerProduct != null ? p.PartnerProduct.ProductName : null,
            p.UnNumber, p.HazardFlags, p.TracksExpiry, p.IsActive, p.CreatedAt, p.UpdatedAt));
}