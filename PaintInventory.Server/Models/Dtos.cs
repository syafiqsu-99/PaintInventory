using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public record PaintItemRequest(
    [property: Required] string Barcode,
    string? Sku,
    string? Name,
    string? ColorCode,
    decimal? Volume,
    string? Unit,
    string? Batch,
    string? Manufacturer,
    decimal? ReorderLevel);

public record PaintItemDto(
    int Id,
    string Barcode,
    string? Sku,
    string? Name,
    string? ColorCode,
    decimal? Volume,
    string? Unit,
    string? Batch,
    string? Manufacturer,
    decimal OnHand,
    decimal? ReorderLevel,
    bool IsLowStock,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ScanRequest(
    [property: Required] string Barcode,
    [property: Required] string Action,
    decimal Quantity,
    string? Unit,
    string? Location,
    string? Notes,
    string? Operator,
    string? DeviceId);

public record ScanResult(
    int ScanId,
    int PaintItemId,
    string Barcode,
    string Action,
    decimal QuantityApplied,
    decimal OnHand,
    bool IsLowStock,
    DateTime Timestamp);

public record InventoryItemDto(
    int Id,
    string Barcode,
    string? Name,
    string? ColorCode,
    string? Unit,
    decimal OnHand,
    decimal? ReorderLevel,
    bool IsLowStock,
    DateTime? UpdatedAt);

public record ScanHistoryDto(
    int Id,
    string Action,
    decimal Quantity,
    string? Unit,
    string? Location,
    string? Operator,
    string? Notes,
    DateTime Timestamp);

public record UsagePointDto(DateTime Date, decimal Quantity);

public record DashboardDto(
    int TotalItems,
    int LowStockCount,
    decimal TotalOnHand,
    IReadOnlyList<UsagePointDto> Usage);