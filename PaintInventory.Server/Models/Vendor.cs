using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public class Vendor
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public bool IsOwnCompany { get; set; }
    public bool StoresStock { get; set; }
    public bool DoesBlasting { get; set; }
    public bool DoesPainting { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<StockBalance> StockBalances { get; set; } = [];
}