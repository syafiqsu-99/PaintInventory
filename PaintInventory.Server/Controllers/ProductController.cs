using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Infrastructure;
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

    [Authorize(Policy = AuthConstants.StaffPolicy)]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductRequest req, CancellationToken ct)
    {
        if (await db.PaintProducts.AnyAsync(p => p.Gtin == req.Gtin, ct))
            return Conflict(new { error = $"GTIN '{req.Gtin}' is already registered." });

        var product = new PaintProduct { Gtin = req.Gtin, CreatedAt = DateTime.UtcNow };
        Apply(product, req);

        db.PaintProducts.Add(product);
        await db.SaveChangesAsync(ct);

        if (req.PartnerProductId is int partnerId)
            await LinkPartnerAsync(product.Id, partnerId, ct);

        var dto = await Project(db.PaintProducts.AsNoTracking().Where(p => p.Id == product.Id)).FirstAsync(ct);
        return CreatedAtAction(nameof(GetByGtin), new { gtin = product.Gtin }, dto);
    }

    [Authorize(Policy = AuthConstants.StaffPolicy)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductRequest req, CancellationToken ct)
    {
        var product = await db.PaintProducts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return NotFound(new { error = $"Product {id} not found." });

        if (product.Gtin != req.Gtin && await db.PaintProducts.AnyAsync(p => p.Gtin == req.Gtin && p.Id != id, ct))
            return Conflict(new { error = $"GTIN '{req.Gtin}' is already registered." });

        Apply(product, req);
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        if (req.PartnerProductId is int partnerId)
            await LinkPartnerAsync(product.Id, partnerId, ct);

        var dto = await Project(db.PaintProducts.AsNoTracking().Where(p => p.Id == id)).FirstAsync(ct);
        return Ok(dto);
    }

    [Authorize(Policy = AuthConstants.StaffPolicy)]
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

    private static void Apply(PaintProduct p, ProductRequest req)
    {
        p.Gtin = req.Gtin;
        p.ItemCode = req.ItemCode;
        p.ProductName = req.ProductName;
        p.Description = req.Description;
        p.Brand = req.Brand;
        p.ProductFamily = req.ProductFamily;
        p.ProductType = req.ProductType;
        p.Component = req.Component;
        p.PackVolume = req.PackVolume;
        p.Unit = req.Unit;
        p.Technology = req.Technology;
        p.Category = req.Category;
        p.SubCategory = req.SubCategory;
        p.Colour = req.Colour;
        p.RalCode = req.RalCode;
        p.GlossLevel = req.GlossLevel;
        p.MixRatio = req.MixRatio;
        p.PotLifeMinutes = req.PotLifeMinutes;
        p.ThinnerProductId = req.ThinnerProductId;
        p.CleanerProductId = req.CleanerProductId;
        p.VolumeSolidsPct = req.VolumeSolidsPct;
        p.VocGramsPerLitre = req.VocGramsPerLitre;
        p.DftMinUm = req.DftMinUm;
        p.DftMaxUm = req.DftMaxUm;
        p.WftMinUm = req.WftMinUm;
        p.WftMaxUm = req.WftMaxUm;
        p.CoverageMinM2L = req.CoverageMinM2L;
        p.CoverageMaxM2L = req.CoverageMaxM2L;
        p.TemperatureResistance = req.TemperatureResistance;
        p.ShelfLifeMonths = req.ShelfLifeMonths;
        p.PartnerProductId = req.PartnerProductId;
        p.UnNumber = req.UnNumber;
        p.HazardFlags = req.HazardFlags;
        p.MsdsUrl = req.MsdsUrl;
        p.TdsUrl = req.TdsUrl;
        p.TracksExpiry = req.TracksExpiry;
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
            p.Id, p.Gtin, p.ItemCode, p.ProductName, p.Description,
            p.Brand, p.ProductFamily, p.ProductType, p.Component,
            p.PackVolume, p.Unit,
            p.Technology, p.Category, p.SubCategory, p.Colour, p.RalCode, p.GlossLevel,
            p.MixRatio, p.PotLifeMinutes,
            p.ThinnerProductId, p.ThinnerProduct != null ? p.ThinnerProduct.ProductName : null,
            p.CleanerProductId, p.CleanerProduct != null ? p.CleanerProduct.ProductName : null,
            p.VolumeSolidsPct, p.VocGramsPerLitre,
            p.DftMinUm, p.DftMaxUm, p.WftMinUm, p.WftMaxUm, p.CoverageMinM2L, p.CoverageMaxM2L,
            p.TemperatureResistance, p.ShelfLifeMonths,
            p.PartnerProductId, p.PartnerProduct != null ? p.PartnerProduct.ProductName : null,
            p.UnNumber, p.HazardFlags, p.MsdsUrl, p.TdsUrl,
            p.TracksExpiry, p.IsActive, p.CreatedAt, p.UpdatedAt));
}