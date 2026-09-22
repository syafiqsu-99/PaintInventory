using System.Globalization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PaintInventory.Server.Models;
using Cell = MigraDoc.DocumentObjectModel.Tables.Cell;

namespace PaintInventory.Server.Services;

public sealed class ReportPdfService
{
    public byte[] Render(ReportDto report)
    {
        var doc = new Document();
        var normal = doc.Styles["Normal"];
        normal.Font.Name = "Arial";
        normal.Font.Size = 8;

        if (report.Items.Count == 0)
        {
            var empty = NewSection(doc);
            AddTitle(empty);
            empty.AddParagraph($"IPO {report.Ipo} — no items.");
        }
        else
        {
            foreach (var item in report.Items)
                AddItemSection(doc, report, item);
        }

        var renderer = new PdfDocumentRenderer { Document = doc };
        renderer.RenderDocument();

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    private static Section NewSection(Document doc)
    {
        var section = doc.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.Orientation = Orientation.Landscape;
        section.PageSetup.TopMargin = Unit.FromCentimeter(1);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(1);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1);
        return section;
    }

    private static void AddItemSection(Document doc, ReportDto report, ReportItemDto item)
    {
        var section = NewSection(doc);
        AddTitle(section);
        AddHeaderBlock(section, report, item);
        AddBlastingBlock(section, item);
        AddCoatsTable(section, item);
        AddTotalsAndTests(section, item);
        AddPreparedBy(section, report);
    }

    private static void AddTitle(Section section)
    {
        var title = section.AddParagraph("PAINT REPORT");
        title.Format.Font.Size = 14;
        title.Format.Font.Bold = true;
        title.Format.Alignment = ParagraphAlignment.Center;

        var sub = section.AddParagraph("Document de suivi peinture");
        sub.Format.Font.Size = 9;
        sub.Format.Font.Italic = true;
        sub.Format.Alignment = ParagraphAlignment.Center;
        sub.Format.SpaceAfter = Unit.FromMillimeter(3);
    }

    private static void AddHeaderBlock(Section section, ReportDto report, ReportItemDto item)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.AddColumn(Unit.FromCentimeter(3));
        table.AddColumn(Unit.FromCentimeter(6));
        table.AddColumn(Unit.FromCentimeter(3));
        table.AddColumn(Unit.FromCentimeter(6));
        table.AddColumn(Unit.FromCentimeter(9));

        var r1 = table.AddRow();
        LabelCell(r1.Cells[0], "IPO");
        ValueCell(r1.Cells[1], report.Ipo);
        LabelCell(r1.Cells[2], "Item");
        ValueCell(r1.Cells[3], item.ItemNo.ToString(CultureInfo.InvariantCulture));
        r1.Cells[4].MergeDown = 2;
        r1.Cells[4].VerticalAlignment = VerticalAlignment.Center;
        var label = r1.Cells[4].AddParagraph();
        label.Format.Alignment = ParagraphAlignment.Center;
        label.AddFormattedText(item.ComponentLabel ?? string.Empty, TextFormat.Bold);
        if (!string.IsNullOrWhiteSpace(item.ComponentDescription))
        {
            var desc = r1.Cells[4].AddParagraph(item.ComponentDescription);
            desc.Format.Alignment = ParagraphAlignment.Center;
            desc.Format.Font.Italic = true;
        }

        var r2 = table.AddRow();
        LabelCell(r2.Cells[0], "Customer");
        ValueCell(r2.Cells[1], report.Customer);
        LabelCell(r2.Cells[2], "Serial No");
        ValueCell(r2.Cells[3], item.SerialNumber);

        var r3 = table.AddRow();
        LabelCell(r3.Cells[0], "Project");
        ValueCell(r3.Cells[1], report.Project);
        LabelCell(r3.Cells[2], "Painting Spec");
        ValueCell(r3.Cells[3], item.PaintingSpec);

        Spacer(section);
    }

    private static void AddBlastingBlock(Section section, ReportItemDto item)
    {
        var head = section.AddParagraph($"Abrasive Blasting: {(item.AbrasiveBlasting ? "Yes" : "No")}");
        head.Format.Font.Bold = true;

        var sp = item.SurfacePrep;
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        double[] widths = [3.5, 2.5, 2.5, 2.0, 2.0, 2.5, 2.5, 3.0, 3.0];
        foreach (var w in widths) table.AddColumn(Unit.FromCentimeter(w));

        var h = table.AddRow();
        string[] headers =
            ["Grade of cleanliness", "Rq'd roughness", "Measured roughness", "Humidity %", "Air °C", "Steel/Substrate °C", "Dew Point °C", "Operator", "Date"];
        for (var i = 0; i < headers.Length; i++) HeaderCell(h.Cells[i], headers[i]);

        var v = table.AddRow();
        ValueCell(v.Cells[0], sp?.GradeOfCleanliness);
        ValueCell(v.Cells[1], sp?.RequiredRoughness);
        ValueCell(v.Cells[2], Num(sp?.MeasuredRoughness));
        ValueCell(v.Cells[3], Num(sp?.HumidityPct));
        ValueCell(v.Cells[4], Num(sp?.AirTempC));
        ValueCell(v.Cells[5], Num(sp?.SubstrateTempC));
        ValueCell(v.Cells[6], Num(sp?.DewPointC));
        ValueCell(v.Cells[7], sp?.Operator);
        ValueCell(v.Cells[8], Date(sp?.Date));

        Spacer(section);
    }

    private static void AddCoatsTable(Section section, ReportItemDto item)
    {
        var head = section.AddParagraph($"Coats ({item.NumberOfCoats})");
        head.Format.Font.Bold = true;

        var table = section.AddTable();
        table.Borders.Width = 0.5;
        double[] widths = [2.0, 3.0, 3.0, 2.2, 1.8, 2.0, 1.6, 1.6, 1.8, 1.6, 2.2, 2.0];
        foreach (var w in widths) table.AddColumn(Unit.FromCentimeter(w));

        var h = table.AddRow();
        string[] headers =
            ["Coat", "Paint ID", "Batch No.", "Shade", "Rq'd thk µm", "Measured µm", "Humidity %", "Air °C", "Substrate °C", "Dew °C", "Operator", "Date"];
        for (var i = 0; i < headers.Length; i++) HeaderCell(h.Cells[i], headers[i]);

        foreach (var c in item.Coats)
        {
            var r = table.AddRow();
            ValueCell(r.Cells[0], CoatLabel(c.CoatType));
            ValueCell(r.Cells[1], c.PartAProductName ?? c.PaintIdText);
            ValueCell(r.Cells[2], Batch(c.PartABatch, c.PartBBatch));
            ValueCell(r.Cells[3], c.Shade);
            ValueCell(r.Cells[4], Num(c.RequiredThicknessUm));
            ValueCell(r.Cells[5], Num(c.MeasuredThicknessUm));
            ValueCell(r.Cells[6], Num(c.HumidityPct));
            ValueCell(r.Cells[7], Num(c.AirTempC));
            ValueCell(r.Cells[8], Num(c.SubstrateTempC));
            ValueCell(r.Cells[9], Num(c.DewPointC));
            ValueCell(r.Cells[10], c.Operator);
            ValueCell(r.Cells[11], Date(c.Date));
        }

        Spacer(section);
    }

    private static void AddTotalsAndTests(Section section, ReportItemDto item)
    {
        var totals = section.AddParagraph();
        totals.AddFormattedText("Required total DFT (µm): ", TextFormat.Bold);
        totals.AddText(Num(item.RequiredTotalDftUm));
        totals.AddText("        ");
        totals.AddFormattedText("Measured total DFT (µm): ", TextFormat.Bold);
        totals.AddText(Num(item.MeasuredTotalDftUm));

        var tests = section.AddParagraph();
        tests.AddFormattedText("Adhesion test: ", TextFormat.Bold);
        tests.AddText((item.AdhesionTestPerformed == true ? "Yes" : "No") + AdhesionType(item.AdhesionTestType));
        if (!string.IsNullOrWhiteSpace(item.MekTestNotes))
        {
            tests.AddText("        ");
            tests.AddFormattedText("MEK: ", TextFormat.Bold);
            tests.AddText(item.MekTestNotes);
        }

        if (!string.IsNullOrWhiteSpace(item.OtherRemarks))
        {
            var remarks = section.AddParagraph();
            remarks.AddFormattedText("Remarks: ", TextFormat.Bold);
            remarks.AddText(item.OtherRemarks);
        }
    }

    private static void AddPreparedBy(Section section, ReportDto report)
    {
        var p = section.AddParagraph();
        p.Format.SpaceBefore = Unit.FromMillimeter(4);
        p.AddFormattedText("Prepared by: ", TextFormat.Bold);
        p.AddText(report.PreparedBy ?? string.Empty);
        p.AddText("            ");
        p.AddFormattedText("Date: ", TextFormat.Bold);
        p.AddText(Date(report.PreparedDate));
    }

    private static void LabelCell(Cell cell, string text)
    {
        var p = cell.AddParagraph(text);
        p.Format.Font.Bold = true;
        p.Format.Font.Size = 7;
        cell.Shading.Color = Colors.WhiteSmoke;
        cell.VerticalAlignment = VerticalAlignment.Center;
    }

    private static void ValueCell(Cell cell, string? text)
    {
        var p = cell.AddParagraph(text ?? string.Empty);
        p.Format.Font.Size = 8;
        cell.VerticalAlignment = VerticalAlignment.Center;
    }

    private static void HeaderCell(Cell cell, string text)
    {
        var p = cell.AddParagraph(text);
        p.Format.Font.Bold = true;
        p.Format.Font.Size = 7;
        p.Format.Alignment = ParagraphAlignment.Center;
        cell.Shading.Color = Colors.LightGray;
        cell.VerticalAlignment = VerticalAlignment.Center;
    }

    private static void Spacer(Section section) =>
        section.AddParagraph().Format.SpaceAfter = Unit.FromMillimeter(2);

    private static string Num(decimal? value) =>
        value?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Date(DateTime? date) =>
        date?.ToString("dd/MMM/yyyy", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string Batch(string? a, string? b)
    {
        var hasA = !string.IsNullOrWhiteSpace(a);
        var hasB = !string.IsNullOrWhiteSpace(b);
        if (hasA && hasB) return $"{a} / {b}";
        if (hasA) return a!;
        if (hasB) return b!;
        return string.Empty;
    }

    private static string CoatLabel(CoatType type) => type switch
    {
        CoatType.Primer => "Primer",
        CoatType.SecondCoat => "2nd coat",
        CoatType.ThirdCoat => "3rd coat",
        CoatType.FourthCoat => "4th coat",
        _ => type.ToString()
    };

    private static string AdhesionType(AdhesionTestType type) => type switch
    {
        AdhesionTestType.TestPlate => " (test plate)",
        AdhesionTestType.ProductionPart => " (production part)",
        _ => string.Empty
    };
}