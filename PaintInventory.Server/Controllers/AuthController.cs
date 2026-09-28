using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Infrastructure;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    PaintInventoryDbContext db,
    IPasswordHasher<Vendor> hasher,
    IConfiguration config,
    UserContext user) : ControllerBase
{
    private const string BootstrapSiteName = "Setup (bootstrap)";

    [AllowAnonymous]
    [HttpGet("sites")]
    public async Task<ActionResult<IEnumerable<LoginSiteDto>>> GetSites(CancellationToken ct)
    {
        var sites = await db.Vendors.AsNoTracking()
            .Where(v => v.IsActive && v.AccessCodeHash != null)
            .OrderBy(v => v.Name)
            .Select(v => new LoginSiteDto(v.Id, v.Name))
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(BootstrapCode))
            sites.Insert(0, new LoginSiteDto(AuthConstants.BootstrapVendorId, BootstrapSiteName));

        return Ok(sites);
    }

    [AllowAnonymous]
    [EnableRateLimiting(AuthConstants.LoginRateLimit)]
    [HttpPost("login")]
    public async Task<ActionResult<CurrentUserDto>> Login(LoginRequest req, CancellationToken ct)
    {
        var operatorName = req.OperatorName.Trim();
        var code = req.AccessCode.Trim();

        CurrentUserDto? signedIn = null;

        if (req.VendorId == AuthConstants.BootstrapVendorId)
        {
            if (!string.IsNullOrWhiteSpace(BootstrapCode) && FixedTimeEquals(code, BootstrapCode))
                signedIn = new CurrentUserDto(AuthConstants.BootstrapVendorId, BootstrapSiteName, operatorName, AuthConstants.StaffRole);
        }
        else
        {
            var vendor = await db.Vendors
                .FirstOrDefaultAsync(v => v.Id == req.VendorId && v.IsActive && v.AccessCodeHash != null, ct);

            if (vendor is not null)
            {
                var result = hasher.VerifyHashedPassword(vendor, vendor.AccessCodeHash!, code);
                if (result != PasswordVerificationResult.Failed)
                {
                    if (result == PasswordVerificationResult.SuccessRehashNeeded)
                        vendor.AccessCodeHash = hasher.HashPassword(vendor, code);

                    var role = vendor.IsOwnCompany ? AuthConstants.StaffRole : AuthConstants.VendorRole;
                    signedIn = new CurrentUserDto(vendor.Id, vendor.Name, operatorName, role);
                }
            }
        }

        AddAudit(signedIn is null ? "LoginFailed" : "Login", operatorName, req.VendorId);
        await db.SaveChangesAsync(ct);

        if (signedIn is null)
            return Unauthorized(new { error = "Invalid site or access code." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, signedIn.Operator),
            new(ClaimTypes.Role, signedIn.Role),
            new(AuthConstants.VendorIdClaim, signedIn.VendorId.ToString()),
            new(AuthConstants.VendorNameClaim, signedIn.VendorName),
            new(AuthConstants.OperatorClaim, signedIn.Operator)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

        return Ok(signedIn);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpGet("me")]
    public ActionResult<CurrentUserDto> Me()
    {
        if (user.VendorId is not int vendorId)
            return Unauthorized();

        return Ok(new CurrentUserDto(
            vendorId,
            user.VendorName ?? string.Empty,
            user.Operator ?? string.Empty,
            user.IsStaff ? AuthConstants.StaffRole : AuthConstants.VendorRole));
    }

    private string? BootstrapCode => config["Auth:BootstrapCode"];

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));

    private void AddAudit(string action, string operatorName, int vendorId) =>
        db.AuditLogs.Add(new AuditLog
        {
            Entity = "Auth",
            EntityId = vendorId.ToString(),
            Action = action,
            ChangedBy = operatorName,
            ChangedAt = DateTime.UtcNow,
            Details = JsonSerializer.Serialize(new { ip = HttpContext.Connection.RemoteIpAddress?.ToString() })
        });
}
