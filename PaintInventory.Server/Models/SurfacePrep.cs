namespace PaintInventory.Server.Models;

public class SurfacePrep
{
    public int Id { get; set; }

    public int PaintReportItemId { get; set; }
    public PaintReportItem ReportItem { get; set; } = null!;

    public string? GradeOfCleanliness { get; set; }
    public string? RequiredRoughness { get; set; }
    public decimal? MeasuredRoughness { get; set; }

    public decimal? HumidityPct { get; set; }
    public decimal? AirTempC { get; set; }
    public decimal? SubstrateTempC { get; set; }
    public decimal? DewPointC { get; set; }

    public string? Operator { get; set; }
    public DateTime? Date { get; set; }
}