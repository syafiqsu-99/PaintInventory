namespace PaintInventory.Server.Models;

public class PaintReportItem
{
    public int Id { get; set; }

    public int PaintReportId { get; set; }
    public PaintReport Report { get; set; } = null!;

    public int ItemNo { get; set; }

    public string? SerialNumber { get; set; }
    public string? PaintingSpec { get; set; }
    public string? ComponentDescription { get; set; }
    public string? ComponentLabel { get; set; }

    public bool AbrasiveBlasting { get; set; }

    public decimal? RequiredTotalDftUm { get; set; }
    public decimal? MeasuredTotalDftUm { get; set; }

    public bool? AdhesionTestPerformed { get; set; }
    public AdhesionTestType AdhesionTestType { get; set; } = AdhesionTestType.None;
    public string? MekTestNotes { get; set; }
    public string? OtherRemarks { get; set; }

    public int? BlastVendorId { get; set; }
    public Vendor? BlastVendor { get; set; }

    public int? PaintingVendorId { get; set; }
    public Vendor? PaintingVendor { get; set; }

    public SurfacePrep? SurfacePrep { get; set; }
    public List<CoatLine> Coats { get; set; } = [];
}