using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public record ProductRequest(
    [Required] string Gtin,
    string? ItemCode,
    [Required] string ProductName,
    string? Description,
    ComponentType Component,
    decimal? PackVolume,
    string? Unit,
    string? DefaultShade,
    string? RalCode,
    string? Manufacturer,
    string? MixRatio,
    int? PartnerProductId,
    string? UnNumber,
    string? HazardFlags,
    bool TracksExpiry);

public record ProductDto(
    int Id,
    string Gtin,
    string? ItemCode,
    string ProductName,
    string? Description,
    ComponentType Component,
    decimal? PackVolume,
    string? Unit,
    string? DefaultShade,
    string? RalCode,
    string? Manufacturer,
    string? MixRatio,
    int? PartnerProductId,
    string? PartnerProductName,
    string? UnNumber,
    string? HazardFlags,
    bool TracksExpiry,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record VendorRequest(
    [Required] string Name,
    bool IsOwnCompany,
    bool StoresStock,
    bool DoesBlasting,
    bool DoesPainting);

public record VendorDto(
    int Id,
    string Name,
    bool IsOwnCompany,
    bool StoresStock,
    bool DoesBlasting,
    bool DoesPainting,
    bool IsActive,
    DateTime CreatedAt);

public record StockInRequest(
    [Required] int ProductId,
    [Required] int VendorId,
    decimal Quantity,
    string? Batch,
    string? Shade,
    decimal? PackVolume,
    DateTime? ManufacturingDate,
    DateTime? BestBefore,
    string? Source,
    string? Operator,
    string? Notes,
    string? DeviceId);

public record StockOutRequest(
    [Required] int ProductId,
    [Required] int VendorId,
    decimal Quantity,
    string? Batch,
    string? Shade,
    int? CoatLineId,
    string? Operator,
    string? Notes,
    string? DeviceId);

public record StockAdjustRequest(
    [Required] int ProductId,
    [Required] int VendorId,
    decimal NewOnHandQty,
    string? Operator,
    string? Notes);

public record StockTransferRequest(
    [Required] int ProductId,
    [Required] int FromVendorId,
    [Required] int ToVendorId,
    decimal Quantity,
    string? Batch,
    string? Operator,
    string? Notes);

public record StockResult(
    int TransactionId,
    int ProductId,
    string Gtin,
    int VendorId,
    string VendorName,
    StockDirection Direction,
    decimal QuantityApplied,
    decimal OnHandQty,
    bool IsLowStock,
    DateTime Timestamp);

public record TransferResult(StockResult From, StockResult To);

public record InventoryLevelDto(
    int BalanceId,
    int ProductId,
    string Gtin,
    string ProductName,
    ComponentType Component,
    string? Shade,
    string? Unit,
    int VendorId,
    string VendorName,
    decimal OnHandQty,
    decimal? ReorderLevel,
    bool IsLowStock,
    DateTime? UpdatedAt);

public record StockHistoryDto(
    int Id,
    StockDirection Direction,
    decimal Quantity,
    string? Batch,
    string VendorName,
    string? CounterpartyVendorName,
    string? Operator,
    string? Notes,
    DateTime Timestamp);

public record SetReorderRequest(
    [Required] int ProductId,
    [Required] int VendorId,
    decimal? ReorderLevel);

public record UsagePointDto(DateTime Date, decimal Quantity);

public record DashboardDto(
    int TotalProducts,
    int LowStockCount,
    decimal TotalOnHand,
    IReadOnlyList<UsagePointDto> Usage);