using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public class LocationHistory
{
    public int Id { get; set; }

    public int PaintItemId { get; set; }
    public PaintItem? PaintItem { get; set; }

    public string Location { get; set; } = string.Empty;
    public decimal QuantityMoved { get; set; }
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
