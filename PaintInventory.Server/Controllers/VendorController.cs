using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Infrastructure;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VendorsController(
    PaintInventoryDbContext db,
    IPasswordHasher<Vendor> hasher,
    UserContext user) : ControllerBase
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

    [Authorize(Policy = AuthConstants.StaffPolicy)]
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

    [Authorize(Policy = AuthConstants.StaffPolicy)]
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

    [Authorize(Policy = AuthConstants.StaffPolicy)]
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

    [Authorize(Policy = AuthConstants.StaffPolicy)]
    [HttpPut("{id:int}/access-code")]
    public async Task<ActionResult<AccessCodeResult>> SetAccessCode(int id, SetAccessCodeRequest req, CancellationToken ct)
    {
        var vendor = await db.Vendors.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (vendor is null)
            return NotFound(new { error = $"Vendor {id} not found." });

        var code = string.IsNullOrWhiteSpace(req.AccessCode) ? GenerateCode() : req.AccessCode.Trim();

        vendor.AccessCodeHash = hasher.HashPassword(vendor, code);
        vendor.AccessCodeUpdatedAt = DateTime.UtcNow;
        db.AuditLogs.Add(new AuditLog
        {
            Entity = nameof(Vendor),
            EntityId = vendor.Id.ToString(),
            Action = "AccessCodeSet",
            ChangedBy = user.Operator,
            ChangedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
        return Ok(new AccessCodeResult(vendor.Id, code));
    }

    [Authorize(Policy = AuthConstants.StaffPolicy)]
    [HttpDelete("{id:int}/access-code")]
    public async Task<IActionResult> RevokeAccessCode(int id, CancellationToken ct)
    {
        var vendor = await db.Vendors.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (vendor is null)
            return NotFound(new { error = $"Vendor {id} not found." });

        vendor.AccessCodeHash = null;
        vendor.AccessCodeUpdatedAt = DateTime.UtcNow;
        db.AuditLogs.Add(new AuditLog
        {
            Entity = nameof(Vendor),
            EntityId = vendor.Id.ToString(),
            Action = "AccessCodeRevoked",
            ChangedBy = user.Operator,
            ChangedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static string GenerateCode() => RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D8");

    private static VendorDto ToDto(Vendor v) =>
        new(v.Id, v.Name, v.IsOwnCompany, v.StoresStock, v.DoesBlasting, v.DoesPainting, v.IsActive, v.CreatedAt,
            v.AccessCodeHash != null);

    private static readonly System.Linq.Expressions.Expression<Func<Vendor, VendorDto>> Projection =
        v => new VendorDto(v.Id, v.Name, v.IsOwnCompany, v.StoresStock, v.DoesBlasting, v.DoesPainting, v.IsActive, v.CreatedAt,
            v.AccessCodeHash != null);
}