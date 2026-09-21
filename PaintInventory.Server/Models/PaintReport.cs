using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public class PaintReport
{
    public int Id { get; set; }

    [Required]
    public string Ipo { get; set; } = string.Empty;

    public string? Customer { get; set; }
    public string? Project { get; set; }

    public string? PreparedBy { get; set; }
    public DateTime? PreparedDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<PaintReportItem> Items { get; set; } = [];
}