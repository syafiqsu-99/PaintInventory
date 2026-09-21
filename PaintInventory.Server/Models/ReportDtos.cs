using System.ComponentModel.DataAnnotations;

namespace PaintInventory.Server.Models;

public record ReportRequest(
    [property: Required] string Ipo,
    string? Customer,
    string? Project,
    string? PreparedBy,
    DateTime? PreparedDate,
    List<ReportItemRequest> Items);

public record ReportItemRequest(
    int ItemNo,
    string? SerialNumber,
    string? PaintingSpec,
    string? ComponentDescription,
    string? ComponentLabel,
    bool AbrasiveBlasting,
    decimal? RequiredTotalDftUm,
    decimal? MeasuredTotalDftUm,
    bool? AdhesionTestPerformed,
    AdhesionTestType AdhesionTestType,
    string? MekTestNotes,
    string? OtherRemarks,
    int? BlastVendorId,
    int? PaintingVendorId,
    SurfacePrepRequest? SurfacePrep,
    List<CoatLineRequest> Coats);

public record SurfacePrepRequest(
    string? GradeOfCleanliness,
    string? RequiredRoughness,
    decimal? MeasuredRoughness,
    decimal? HumidityPct,
    decimal? AirTempC,
    decimal? SubstrateTempC,
    decimal? DewPointC,
    string? Operator,
    DateTime? Date);

public record CoatLineRequest(
    CoatType CoatType,
    int Sequence,
    int? PartAProductId,
    int? PartBProductId,
    string? PaintIdText,
    string? PartABatch,
    string? PartBBatch,
    string? Shade,
    decimal? RequiredThicknessUm,
    decimal? MeasuredThicknessUm,
    decimal? HumidityPct,
    decimal? AirTempC,
    decimal? SubstrateTempC,
    decimal? DewPointC,
    string? Operator,
    DateTime? Date,
    bool DeductFromStock,
    int? StockLocationVendorId,
    decimal? PartAQtyUsed,
    decimal? PartBQtyUsed);

public record ReportListItemDto(
    int Id,
    string Ipo,
    string? Customer,
    string? Project,
    int ItemCount,
    string? PreparedBy,
    DateTime? PreparedDate,
    DateTime CreatedAt);

public record ReportDto(
    int Id,
    string Ipo,
    string? Customer,
    string? Project,
    string? PreparedBy,
    DateTime? PreparedDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<ReportItemDto> Items);

public record ReportItemDto(
    int Id,
    int ItemNo,
    string? SerialNumber,
    string? PaintingSpec,
    string? ComponentDescription,
    string? ComponentLabel,
    bool AbrasiveBlasting,
    int NumberOfCoats,
    decimal? RequiredTotalDftUm,
    decimal? MeasuredTotalDftUm,
    bool? AdhesionTestPerformed,
    AdhesionTestType AdhesionTestType,
    string? MekTestNotes,
    string? OtherRemarks,
    int? BlastVendorId,
    string? BlastVendorName,
    int? PaintingVendorId,
    string? PaintingVendorName,
    SurfacePrepDto? SurfacePrep,
    List<CoatLineDto> Coats);

public record SurfacePrepDto(
    string? GradeOfCleanliness,
    string? RequiredRoughness,
    decimal? MeasuredRoughness,
    decimal? HumidityPct,
    decimal? AirTempC,
    decimal? SubstrateTempC,
    decimal? DewPointC,
    string? Operator,
    DateTime? Date);

public record CoatLineDto(
    int Id,
    CoatType CoatType,
    int Sequence,
    int? PartAProductId,
    string? PartAProductName,
    int? PartBProductId,
    string? PartBProductName,
    string? PaintIdText,
    string? PartABatch,
    string? PartBBatch,
    string? Shade,
    decimal? RequiredThicknessUm,
    decimal? MeasuredThicknessUm,
    decimal? HumidityPct,
    decimal? AirTempC,
    decimal? SubstrateTempC,
    decimal? DewPointC,
    string? Operator,
    DateTime? Date,
    bool DeductFromStock,
    int? StockLocationVendorId,
    decimal? PartAQtyUsed,
    decimal? PartBQtyUsed);

public record ReportSaveResult(ReportDto Report, List<string> Warnings);