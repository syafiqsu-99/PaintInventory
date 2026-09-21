using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public class StockBalance
{
    public int Id { get; set; }

    public int PaintProductId { get; set; }
    public PaintProduct PaintProduct { get; set; } = null!;

    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;

    public decimal OnHandQty { get; set; }
    public decimal? ReorderLevel { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}