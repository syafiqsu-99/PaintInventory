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

    public Brand Brand { get; set; } = Brand.Jotun;
    public string? ProductFamily { get; set; }

    public ProductType ProductType { get; set; } = ProductType.Coating;
    public ComponentType Component { get; set; } = ComponentType.Single;

    public decimal? PackVolume { get; set; }
    public string? Unit { get; set; }

    public string? Technology { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Colour { get; set; }
    public string? RalCode { get; set; }
    public GlossLevel? GlossLevel { get; set; }

    public string? MixRatio { get; set; }
    public int? PotLifeMinutes { get; set; }

    public int? ThinnerProductId { get; set; }
    public PaintProduct? ThinnerProduct { get; set; }

    public int? CleanerProductId { get; set; }
    public PaintProduct? CleanerProduct { get; set; }

    public decimal? VolumeSolidsPct { get; set; }
    public decimal? VocGramsPerLitre { get; set; }

    public int? DftMinUm { get; set; }
    public int? DftMaxUm { get; set; }
    public int? WftMinUm { get; set; }
    public int? WftMaxUm { get; set; }

    public decimal? CoverageMinM2L { get; set; }
    public decimal? CoverageMaxM2L { get; set; }

    public string? TemperatureResistance { get; set; }

    public int? ShelfLifeMonths { get; set; }

    public int? PartnerProductId { get; set; }
    public PaintProduct? PartnerProduct { get; set; }

    public string? UnNumber { get; set; }
    public string? HazardFlags { get; set; }

    public string? MsdsUrl { get; set; }
    public string? TdsUrl { get; set; }

    public bool TracksExpiry { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<StockBalance> StockBalances { get; set; } = [];
    public List<StockTransaction> StockTransactions { get; set; } = [];
}