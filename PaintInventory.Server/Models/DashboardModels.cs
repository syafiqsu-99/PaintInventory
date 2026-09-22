namespace PaintInventory.Server.Models;

public record LocationStockDto(string VendorName, decimal OnHand);

public record TopUsedDto(string ProductName, decimal Quantity);

public record ExpiringLotDto(
    int ProductId,
    string Gtin,
    string ProductName,
    string VendorName,
    string? Batch,
    DateTime BestBefore,
    int DaysToExpiry,
    decimal Quantity);

public record DashboardSummaryDto(
    int TotalProducts,
    int TotalLocations,
    int LowStockCount,
    int OutOfStockCount,
    int ExpiringSoonCount,
    decimal TotalOnHand,
    int ExpiryWindowDays,
    IReadOnlyList<UsagePointDto> Usage,
    IReadOnlyList<LocationStockDto> StockByLocation,
    IReadOnlyList<TopUsedDto> TopUsed,
    IReadOnlyList<ExpiringLotDto> ExpiringSoon,
    IReadOnlyList<InventoryLevelDto> LowStock);