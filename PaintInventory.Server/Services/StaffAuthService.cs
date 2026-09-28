using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Services;

public sealed class StaffAuthService(
    PaintInventoryDbContext db,
    IPasswordHasher<AppSetting> hasher,
    IConfiguration config)
{
    public const string PasswordKey = "StaffPasswordHash";

    private string? FallbackPassword => config["Auth:StaffPassword"];

    public async Task<bool> IsConfiguredAsync(CancellationToken ct) =>
        !string.IsNullOrWhiteSpace(FallbackPassword)
        || await db.AppSettings.AnyAsync(s => s.Key == PasswordKey && s.Value != null, ct);

    public async Task<bool> VerifyAsync(string password, CancellationToken ct)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == PasswordKey, ct);

        if (!string.IsNullOrEmpty(setting?.Value))
        {
            var result = hasher.VerifyHashedPassword(setting, setting.Value, password);
            if (result == PasswordVerificationResult.Failed)
                return false;
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                setting.Value = hasher.HashPassword(setting, password);
                setting.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
            }
            return true;
        }

        var fallback = FallbackPassword;
        return !string.IsNullOrEmpty(fallback)
            && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(fallback));
    }

    public async Task<bool> ChangeAsync(string currentPassword, string newPassword, CancellationToken ct)
    {
        if (!await VerifyAsync(currentPassword, ct))
            return false;

        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == PasswordKey, ct);
        if (setting is null)
        {
            setting = new AppSetting { Key = PasswordKey };
            db.AppSettings.Add(setting);
        }

        setting.Value = hasher.HashPassword(setting, newPassword);
        setting.UpdatedAt = DateTime.UtcNow;

        db.AuditLogs.Add(new AuditLog
        {
            Entity = "Auth",
            Action = "StaffPasswordChanged",
            ChangedBy = "Staff",
            ChangedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task LogAttemptAsync(bool success, string? ip, CancellationToken ct)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Entity = "Auth",
            Action = success ? "Unlock" : "UnlockFailed",
            ChangedBy = "Staff",
            ChangedAt = DateTime.UtcNow,
            Details = JsonSerializer.Serialize(new { ip })
        });
        await db.SaveChangesAsync(ct);
    }
}
