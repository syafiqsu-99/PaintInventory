using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public record UnlockRequest([Required] string Password);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, StringLength(128, MinimumLength = 6)] string NewPassword);

public record StaffStatusDto(bool IsStaff, bool PasswordConfigured);
