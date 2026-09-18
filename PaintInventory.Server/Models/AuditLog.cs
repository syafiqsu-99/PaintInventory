namespace PaintInventory.Server.Models;

public class AuditLog
{
    public int Id { get; set; }

    public string Entity { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; } // JSON payload of change
}
