namespace PaintInventory.Server.Models;

public class CoatLine
{
    public int Id { get; set; }

    public int PaintReportItemId { get; set; }
    public PaintReportItem ReportItem { get; set; } = null!;

    public CoatType CoatType { get; set; }
    public int Sequence { get; set; }

    public int? PartAProductId { get; set; }
    public PaintProduct? PartAProduct { get; set; }

    public int? PartBProductId { get; set; }
    public PaintProduct? PartBProduct { get; set; }

    public string? PaintIdText { get; set; }
    public string? PartABatch { get; set; }
    public string? PartBBatch { get; set; }
    public string? Shade { get; set; }

    public decimal? RequiredThicknessUm { get; set; }
    public decimal? MeasuredThicknessUm { get; set; }

    public decimal? HumidityPct { get; set; }
    public decimal? AirTempC { get; set; }
    public decimal? SubstrateTempC { get; set; }
    public decimal? DewPointC { get; set; }

    public string? Operator { get; set; }
    public DateTime? Date { get; set; }

    public bool DeductFromStock { get; set; }
    public int? StockLocationVendorId { get; set; }
    public Vendor? StockLocationVendor { get; set; }

    public decimal? PartAQtyUsed { get; set; }
    public decimal? PartBQtyUsed { get; set; }

    public List<StockTransaction> StockTransactions { get; set; } = [];
}