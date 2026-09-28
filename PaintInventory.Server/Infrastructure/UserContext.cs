using System.Security.Claims;

namespace PaintInventory.Server.Infrastructure;

public static class AuthConstants
{
    public const string StaffRole = "Staff";
    public const string VendorRole = "Vendor";
    public const string StaffPolicy = "Staff";
    public const string LoginRateLimit = "login";
    public const string VendorIdClaim = "pi:vendor_id";
    public const string VendorNameClaim = "pi:vendor_name";
    public const string OperatorClaim = "pi:operator";
    public const int BootstrapVendorId = 0;
}

public sealed class UserContext(IHttpContextAccessor accessor)
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public bool IsStaff => User?.IsInRole(AuthConstants.StaffRole) == true;

    public int? VendorId =>
        int.TryParse(User?.FindFirstValue(AuthConstants.VendorIdClaim), out var id) ? id : null;

    public string? VendorName => User?.FindFirstValue(AuthConstants.VendorNameClaim);

    public string? Operator => User?.FindFirstValue(AuthConstants.OperatorClaim);
}
