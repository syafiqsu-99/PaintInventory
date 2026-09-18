using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public class PaintItem
{
    public int Id { get; set; }

    [Required]
    public string Barcode { get; set; } = string.Empty;

    public string? SKU { get; set; }
    public string? Name { get; set; }
    public string? ColorCode { get; set; }
    public decimal? Volume { get; set; }
    public string? Unit { get; set; }
    public string? Batch { get; set; }
    public string? Manufacturer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public List<ScanRecord> ScanRecords { get; set; } = new();
    public List<LocationHistory> LocationHistories { get; set; } = new();
}
