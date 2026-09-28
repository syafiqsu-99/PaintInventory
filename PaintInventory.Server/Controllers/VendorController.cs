using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VendorsController(PaintInventoryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetAll(
        [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var q = db.Vendors.AsNoTracking();
        if (!includeInactive) q = q.Where(v => v.IsActive);
        var vendors = await q.OrderBy(v => v.Name).Select(Projection).ToListAsync(ct);
        return Ok(vendors);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VendorDto>> GetById(int id, CancellationToken ct)
    {
        var vendor = await db.Vendors.AsNoTracking().Where(v => v.Id == id).Select(Projection).FirstOrDefaultAsync(ct);
        return vendor is null ? NotFound(new { error = $"Vendor {id} not found." }) : Ok(vendor);
    }

    [HttpPost]
    public async Task<ActionResult<VendorDto>> Create(VendorRequest req, CancellationToken ct)
    {
        if (await db.Vendors.AnyAsync(v => v.Name == req.Name, ct))
            return Conflict(new { error = $"Vendor '{req.Name}' already exists." });

        var vendor = new Vendor
        {
            Name = req.Name,
            IsOwnCompany = req.IsOwnCompany,
            StoresStock = req.StoresStock,
            DoesBlasting = req.DoesBlasting,
            DoesPainting = req.DoesPainting,
            CreatedAt = DateTime.UtcNow
        };

        db.Vendors.Add(vendor);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = vendor.Id }, ToDto(vendor));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VendorDto>> Update(int id, VendorRequest req, CancellationToken ct)
    {
        var vendor = await db.Vendors.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (vendor is null)
            return NotFound(new { error = $"Vendor {id} not found." });

        if (vendor.Name != req.Name && await db.Vendors.AnyAsync(v => v.Name == req.Name && v.Id != id, ct))
            return Conflict(new { error = $"Vendor '{req.Name}' already exists." });

        vendor.Name = req.Name;
        vendor.IsOwnCompany = req.IsOwnCompany;
        vendor.StoresStock = req.StoresStock;
        vendor.DoesBlasting = req.DoesBlasting;
        vendor.DoesPainting = req.DoesPainting;

        await db.SaveChangesAsync(ct);
        return Ok(ToDto(vendor));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var vendor = await db.Vendors.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (vendor is null)
            return NotFound(new { error = $"Vendor {id} not found." });

        vendor.IsActive = false;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static VendorDto ToDto(Vendor v) =>
        new(v.Id, v.Name, v.IsOwnCompany, v.StoresStock, v.DoesBlasting, v.DoesPainting, v.IsActive, v.CreatedAt);

    private static readonly System.Linq.Expressions.Expression<Func<Vendor, VendorDto>> Projection =
        v => new VendorDto(v.Id, v.Name, v.IsOwnCompany, v.StoresStock, v.DoesBlasting, v.DoesPainting, v.IsActive, v.CreatedAt);
}