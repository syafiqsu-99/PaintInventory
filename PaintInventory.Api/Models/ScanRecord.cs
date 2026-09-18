using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Api.Models;

public class ScanRecord
{
    public int Id { get; set; }

    public int? PaintItemId { get; set; }
    public PaintItem? PaintItem { get; set; }

    [Required]
    public string BarcodeScanned { get; set; } = string.Empty;

    [Required]
    public string Action { get; set; } = "Use"; // Use, Receive, Adjust

    public decimal Quantity { get; set; }
    public string? Unit { get; set; }

    public string? DeviceId { get; set; }
    public string? Operator { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
