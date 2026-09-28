using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public record LoginRequest(
    int VendorId,
    [Required] string AccessCode,
    [Required, StringLength(100, MinimumLength = 2)] string OperatorName);

public record LoginSiteDto(int Id, string Name);

public record CurrentUserDto(int VendorId, string VendorName, string Operator, string Role);

public record SetAccessCodeRequest([StringLength(32, MinimumLength = 6)] string? AccessCode);

public record AccessCodeResult(int VendorId, string AccessCode);
