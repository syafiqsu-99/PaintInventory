using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PaintInventory.Server.Infrastructure;
using PaintInventory.Server.Models;
using PaintInventory.Server.Services;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(StaffAuthService staff) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("me")]
    public async Task<ActionResult<StaffStatusDto>> Me(CancellationToken ct) =>
        Ok(new StaffStatusDto(User.IsInRole(AuthConstants.StaffRole), await staff.IsConfiguredAsync(ct)));

    [AllowAnonymous]
    [EnableRateLimiting(AuthConstants.UnlockRateLimit)]
    [HttpPost("unlock")]
    public async Task<ActionResult<StaffStatusDto>> Unlock(UnlockRequest req, CancellationToken ct)
    {
        var ok = await staff.VerifyAsync(req.Password, ct);
        await staff.LogAttemptAsync(ok, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);

        if (!ok)
            return Unauthorized(new { error = "Incorrect password." });

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "Staff"), new Claim(ClaimTypes.Role, AuthConstants.StaffRole)],
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = false });

        return Ok(new StaffStatusDto(true, true));
    }

    [AllowAnonymous]
    [HttpPost("lock")]
    public async Task<IActionResult> Lock()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req, CancellationToken ct)
    {
        if (!await staff.ChangeAsync(req.CurrentPassword, req.NewPassword, ct))
            return BadRequest(new { error = "Current password is incorrect." });
        return NoContent();
    }
}
