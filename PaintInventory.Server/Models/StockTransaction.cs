namespace PaintInventory.Server.Models;

public class StockTransaction
{
    public int Id { get; set; }

    public int PaintProductId { get; set; }
    public PaintProduct PaintProduct { get; set; } = null!;

    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;

    public int? CounterpartyVendorId { get; set; }
    public Vendor? CounterpartyVendor { get; set; }

    public StockDirection Direction { get; set; }
    public decimal Quantity { get; set; }

    public string? Batch { get; set; }
    public string? Shade { get; set; }
    public decimal? PackVolume { get; set; }

    public DateTime? ManufacturingDate { get; set; }
    public DateTime? BestBefore { get; set; }

    public string? Source { get; set; }

    public int? CoatLineId { get; set; }
    public CoatLine? CoatLine { get; set; }

    public string? Operator { get; set; }
    public string? Notes { get; set; }
    public string? DeviceId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}