using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Services;

public sealed class ReportService(PaintInventoryDbContext db, StockService stock)
{
    public async Task<List<ReportListItemDto>> ListAsync(CancellationToken ct) =>
        await db.PaintReports.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReportListItemDto(
                r.Id, r.Ipo, r.Customer, r.Project, r.Items.Count, r.PreparedBy, r.PreparedDate, r.CreatedAt))
            .ToListAsync(ct);

    public async Task<ReportDto?> GetAsync(int id, CancellationToken ct) =>
        await db.PaintReports.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReportDto(
                r.Id, r.Ipo, r.Customer, r.Project, r.PreparedBy, r.PreparedDate, r.CreatedAt, r.UpdatedAt,
                r.Items.OrderBy(i => i.ItemNo).Select(i => new ReportItemDto(
                    i.Id, i.ItemNo, i.SerialNumber, i.PaintingSpec, i.ComponentDescription, i.ComponentLabel,
                    i.AbrasiveBlasting, i.Coats.Count, i.RequiredTotalDftUm, i.MeasuredTotalDftUm,
                    i.AdhesionTestPerformed, i.AdhesionTestType, i.MekTestNotes, i.OtherRemarks,
                    i.BlastVendorId, i.BlastVendor != null ? i.BlastVendor.Name : null,
                    i.PaintingVendorId, i.PaintingVendor != null ? i.PaintingVendor.Name : null,
                    i.SurfacePrep == null ? null : new SurfacePrepDto(
                        i.SurfacePrep.GradeOfCleanliness, i.SurfacePrep.RequiredRoughness, i.SurfacePrep.MeasuredRoughness,
                        i.SurfacePrep.HumidityPct, i.SurfacePrep.AirTempC, i.SurfacePrep.SubstrateTempC,
                        i.SurfacePrep.DewPointC, i.SurfacePrep.Operator, i.SurfacePrep.Date),
                    i.Coats.OrderBy(c => c.Sequence).Select(c => new CoatLineDto(
                        c.Id, c.CoatType, c.Sequence,
                        c.PartAProductId, c.PartAProduct != null ? c.PartAProduct.ProductName : null,
                        c.PartBProductId, c.PartBProduct != null ? c.PartBProduct.ProductName : null,
                        c.PaintIdText, c.PartABatch, c.PartBBatch, c.Shade,
                        c.RequiredThicknessUm, c.MeasuredThicknessUm, c.HumidityPct, c.AirTempC, c.SubstrateTempC,
                        c.DewPointC, c.Operator, c.Date, c.DeductFromStock, c.StockLocationVendorId,
                        c.PartAQtyUsed, c.PartBQtyUsed)).ToList()))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

    public async Task<ReportSaveResult> CreateAsync(ReportRequest req, CancellationToken ct)
    {
        var report = new PaintReport
        {
            Ipo = req.Ipo,
            Customer = req.Customer,
            Project = req.Project,
            PreparedBy = req.PreparedBy,
            PreparedDate = req.PreparedDate,
            CreatedAt = DateTime.UtcNow
        };
        foreach (var i in req.Items)
            report.Items.Add(BuildItem(i));

        db.PaintReports.Add(report);
        await db.SaveChangesAsync(ct);

        var warnings = await ApplyStockDeductionsAsync(report, ct);
        var dto = await GetAsync(report.Id, ct);
        return new ReportSaveResult(dto!, warnings);
    }

    public async Task<ReportSaveResult?> UpdateAsync(int id, ReportRequest req, CancellationToken ct)
    {
        var report = await db.PaintReports
            .Include(r => r.Items).ThenInclude(i => i.SurfacePrep)
            .Include(r => r.Items).ThenInclude(i => i.Coats)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (report is null) return null;

        report.Ipo = req.Ipo;
        report.Customer = req.Customer;
        report.Project = req.Project;
        report.PreparedBy = req.PreparedBy;
        report.PreparedDate = req.PreparedDate;
        report.UpdatedAt = DateTime.UtcNow;

        db.PaintReportItems.RemoveRange(report.Items);
        report.Items.Clear();
        foreach (var i in req.Items)
            report.Items.Add(BuildItem(i));

        await db.SaveChangesAsync(ct);

        var warnings = new List<string>();
        if (req.Items.SelectMany(i => i.Coats).Any(c => c.DeductFromStock))
            warnings.Add("Stock consumption is posted only when a report is first created; edits do not re-post. Adjust stock manually if needed.");

        var dto = await GetAsync(report.Id, ct);
        return new ReportSaveResult(dto!, warnings);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var report = await db.PaintReports.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (report is null) return false;
        db.PaintReports.Remove(report);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static PaintReportItem BuildItem(ReportItemRequest i)
    {
        var item = new PaintReportItem
        {
            ItemNo = i.ItemNo,
            SerialNumber = i.SerialNumber,
            PaintingSpec = i.PaintingSpec,
            ComponentDescription = i.ComponentDescription,
            ComponentLabel = i.ComponentLabel,
            AbrasiveBlasting = i.AbrasiveBlasting,
            RequiredTotalDftUm = i.RequiredTotalDftUm,
            MeasuredTotalDftUm = i.MeasuredTotalDftUm,
            AdhesionTestPerformed = i.AdhesionTestPerformed,
            AdhesionTestType = i.AdhesionTestType,
            MekTestNotes = i.MekTestNotes,
            OtherRemarks = i.OtherRemarks,
            BlastVendorId = i.BlastVendorId,
            PaintingVendorId = i.PaintingVendorId
        };

        if (i.SurfacePrep is { } sp)
        {
            item.SurfacePrep = new SurfacePrep
            {
                GradeOfCleanliness = sp.GradeOfCleanliness,
                RequiredRoughness = sp.RequiredRoughness,
                MeasuredRoughness = sp.MeasuredRoughness,
                HumidityPct = sp.HumidityPct,
                AirTempC = sp.AirTempC,
                SubstrateTempC = sp.SubstrateTempC,
                DewPointC = sp.DewPointC ?? DewPoint.Calculate(sp.AirTempC, sp.HumidityPct),
                Operator = sp.Operator,
                Date = sp.Date
            };
        }

        foreach (var c in i.Coats.OrderBy(x => x.Sequence))
        {
            item.Coats.Add(new CoatLine
            {
                CoatType = c.CoatType,
                Sequence = c.Sequence,
                PartAProductId = c.PartAProductId,
                PartBProductId = c.PartBProductId,
                PaintIdText = c.PaintIdText,
                PartABatch = c.PartABatch,
                PartBBatch = c.PartBBatch,
                Shade = c.Shade,
                RequiredThicknessUm = c.RequiredThicknessUm,
                MeasuredThicknessUm = c.MeasuredThicknessUm,
                HumidityPct = c.HumidityPct,
                AirTempC = c.AirTempC,
                SubstrateTempC = c.SubstrateTempC,
                DewPointC = c.DewPointC ?? DewPoint.Calculate(c.AirTempC, c.HumidityPct),
                Operator = c.Operator,
                Date = c.Date,
                DeductFromStock = c.DeductFromStock,
                StockLocationVendorId = c.StockLocationVendorId,
                PartAQtyUsed = c.PartAQtyUsed,
                PartBQtyUsed = c.PartBQtyUsed
            });
        }

        return item;
    }

    private async Task<List<string>> ApplyStockDeductionsAsync(PaintReport report, CancellationToken ct)
    {
        var warnings = new List<string>();

        foreach (var item in report.Items)
            foreach (var coat in item.Coats.Where(c => c.DeductFromStock))
            {
                if (coat.StockLocationVendorId is not int locationId)
                {
                    warnings.Add($"Item {item.ItemNo} coat {coat.Sequence}: no stock location set, consumption not posted.");
                    continue;
                }

                await DeductAsync(coat.PartAProductId, coat.PartAQtyUsed, coat.PartABatch, coat, locationId, "Part A", warnings, ct);
                await DeductAsync(coat.PartBProductId, coat.PartBQtyUsed, coat.PartBBatch, coat, locationId, "Part B", warnings, ct);
            }

        return warnings;
    }

    private async Task DeductAsync(
        int? productId, decimal? qty, string? batch, CoatLine coat, int locationId,
        string label, List<string> warnings, CancellationToken ct)
    {
        if (productId is not int pid || qty is not decimal q || q <= 0) return;

        var outcome = await stock.StockOutAsync(
            new StockOutRequest(pid, locationId, q, batch, coat.Shade, coat.Id, coat.Operator, "Coat consumption", null), ct);

        if (outcome.Status != StockOpStatus.Ok)
            warnings.Add($"Coat {coat.Sequence} {label}: {outcome.Error}");
    }
}