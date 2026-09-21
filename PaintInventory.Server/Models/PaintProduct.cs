using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public class PaintProduct
{
    public int Id { get; set; }

    [Required]
    public string Gtin { get; set; } = string.Empty;

    public string? ItemCode { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ComponentType Component { get; set; } = ComponentType.Single;

    public decimal? PackVolume { get; set; }
    public string? Unit { get; set; }

    public string? DefaultShade { get; set; }
    public string? RalCode { get; set; }

    public string? Manufacturer { get; set; }
    public string? MixRatio { get; set; }

    public int? PartnerProductId { get; set; }
    public PaintProduct? PartnerProduct { get; set; }

    public string? UnNumber { get; set; }
    public string? HazardFlags { get; set; }

    public bool TracksExpiry { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<StockBalance> StockBalances { get; set; } = [];
    public List<StockTransaction> StockTransactions { get; set; } = [];
}