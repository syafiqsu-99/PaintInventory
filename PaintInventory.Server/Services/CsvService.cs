using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Services;

public sealed class CsvService(PaintInventoryDbContext db)
{
    public static readonly string[] ProductColumns =
    [
        "Gtin", "ItemCode", "ProductName", "Description", "Component", "PackVolume",
        "Unit", "DefaultShade", "RalCode", "Manufacturer", "MixRatio",
        "UnNumber", "HazardFlags", "TracksExpiry"
    ];

    public static readonly string[] VendorColumns =
        ["Name", "IsOwnCompany", "StoresStock", "DoesBlasting", "DoesPainting"];

    // ---------- Export ----------

    public async Task<byte[]> ExportProductsAsync(CancellationToken ct)
    {
        var rows = await db.PaintProducts.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProductName)
            .Select(p => new[]
            {
                p.Gtin, p.ItemCode ?? "", p.ProductName, p.Description ?? "", p.Component.ToString(),
                p.PackVolume.HasValue ? p.PackVolume.Value.ToString(CultureInfo.InvariantCulture) : "",
                p.Unit ?? "", p.DefaultShade ?? "", p.RalCode ?? "", p.Manufacturer ?? "", p.MixRatio ?? "",
                p.UnNumber ?? "", p.HazardFlags ?? "", p.TracksExpiry ? "true" : "false"
            })
            .ToListAsync(ct);

        return Write(ProductColumns, rows);
    }

    public async Task<byte[]> ExportVendorsAsync(CancellationToken ct)
    {
        var rows = await db.Vendors.AsNoTracking()
            .Where(v => v.IsActive)
            .OrderBy(v => v.Name)
            .Select(v => new[]
            {
                v.Name, v.IsOwnCompany ? "true" : "false", v.StoresStock ? "true" : "false",
                v.DoesBlasting ? "true" : "false", v.DoesPainting ? "true" : "false"
            })
            .ToListAsync(ct);

        return Write(VendorColumns, rows);
    }

    public byte[] Template(string[] columns) => Write(columns, []);

    // ---------- Products import ----------

    public async Task<ImportPreviewDto> PreviewProductsAsync(Stream file, CancellationToken ct)
    {
        var table = Read(file, ProductColumns);
        var existingGtins = await db.PaintProducts.AsNoTracking().Select(p => p.Gtin).ToListAsync(ct);
        var existing = new HashSet<string>(existingGtins, StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rows = new List<ImportRowDto>();
        foreach (var (values, index) in table.Select((v, i) => (v, i)))
        {
            var (status, message) = ValidateProductRow(values, existing, seen);
            rows.Add(new ImportRowDto(index + 2, status, message, values));
        }

        return Summarise("Products", ProductColumns, rows);
    }

    public async Task<ImportResultDto> CommitProductsAsync(Stream file, ImportMode mode, CancellationToken ct)
    {
        var table = Read(file, ProductColumns);
        var existing = await db.PaintProducts.ToDictionaryAsync(p => p.Gtin, StringComparer.OrdinalIgnoreCase, ct);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existingKeys = new HashSet<string>(existing.Keys, StringComparer.OrdinalIgnoreCase);

        int inserted = 0, updated = 0, skipped = 0, invalid = 0;

        foreach (var values in table)
        {
            var (status, _) = ValidateProductRow(values, existingKeys, seen);
            if (status == ImportRowStatus.Invalid) { invalid++; continue; }

            var gtin = values["Gtin"]!.Trim();
            if (status == ImportRowStatus.Duplicate)
            {
                if (mode == ImportMode.SkipDuplicates) { skipped++; continue; }
                ApplyProduct(existing[gtin], values);
                existing[gtin].UpdatedAt = DateTime.UtcNow;
                updated++;
            }
            else
            {
                var product = new PaintProduct { Gtin = gtin, CreatedAt = DateTime.UtcNow };
                ApplyProduct(product, values);
                db.PaintProducts.Add(product);
                inserted++;
            }
        }

        await db.SaveChangesAsync(ct);
        return new ImportResultDto("Products", inserted, updated, skipped, invalid, []);
    }

    private static (ImportRowStatus, string?) ValidateProductRow(
        IReadOnlyDictionary<string, string?> v, HashSet<string> existing, HashSet<string> seen)
    {
        var gtin = v["Gtin"]?.Trim();
        var name = v["ProductName"]?.Trim();

        if (string.IsNullOrWhiteSpace(gtin)) return (ImportRowStatus.Invalid, "GTIN is required.");
        if (string.IsNullOrWhiteSpace(name)) return (ImportRowStatus.Invalid, "Product name is required.");

        var component = v["Component"]?.Trim();
        if (!string.IsNullOrWhiteSpace(component) && !Enum.TryParse<ComponentType>(component, true, out _))
            return (ImportRowStatus.Invalid, $"Component '{component}' must be Single, PartA or PartB.");

        var pack = v["PackVolume"]?.Trim();
        if (!string.IsNullOrWhiteSpace(pack) &&
            !decimal.TryParse(pack, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            return (ImportRowStatus.Invalid, $"Pack volume '{pack}' is not a number.");

        if (!seen.Add(gtin))
            return (ImportRowStatus.Duplicate, "GTIN appears more than once in this file.");
        if (existing.Contains(gtin))
            return (ImportRowStatus.Duplicate, "GTIN already exists in the database.");

        return (ImportRowStatus.New, null);
    }

    private static void ApplyProduct(PaintProduct p, IReadOnlyDictionary<string, string?> v)
    {
        p.ItemCode = Blank(v["ItemCode"]);
        p.ProductName = v["ProductName"]!.Trim();
        p.Description = Blank(v["Description"]);
        p.Component = Enum.TryParse<ComponentType>(v["Component"]?.Trim(), true, out var c) ? c : ComponentType.Single;
        p.PackVolume = ParseDecimal(v["PackVolume"]);
        p.Unit = Blank(v["Unit"]);
        p.DefaultShade = Blank(v["DefaultShade"]);
        p.RalCode = Blank(v["RalCode"]);
        p.Manufacturer = Blank(v["Manufacturer"]);
        p.MixRatio = Blank(v["MixRatio"]);
        p.UnNumber = Blank(v["UnNumber"]);
        p.HazardFlags = Blank(v["HazardFlags"]);
        p.TracksExpiry = ParseBool(v["TracksExpiry"]);
    }

    // ---------- Vendors import ----------

    public async Task<ImportPreviewDto> PreviewVendorsAsync(Stream file, CancellationToken ct)
    {
        var table = Read(file, VendorColumns);
        var existingNames = await db.Vendors.AsNoTracking().Select(x => x.Name).ToListAsync(ct);
        var existing = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rows = new List<ImportRowDto>();
        foreach (var (values, index) in table.Select((v, i) => (v, i)))
        {
            var (status, message) = ValidateVendorRow(values, existing, seen);
            rows.Add(new ImportRowDto(index + 2, status, message, values));
        }

        return Summarise("Locations", VendorColumns, rows);
    }

    public async Task<ImportResultDto> CommitVendorsAsync(Stream file, ImportMode mode, CancellationToken ct)
    {
        var table = Read(file, VendorColumns);
        var existing = await db.Vendors.ToListAsync(ct);
        var byName = existing.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var existingKeys = new HashSet<string>(byName.Keys, StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        int inserted = 0, updated = 0, skipped = 0, invalid = 0;

        foreach (var values in table)
        {
            var (status, _) = ValidateVendorRow(values, existingKeys, seen);
            if (status == ImportRowStatus.Invalid) { invalid++; continue; }

            var name = values["Name"]!.Trim();
            if (status == ImportRowStatus.Duplicate)
            {
                if (mode == ImportMode.SkipDuplicates) { skipped++; continue; }
                ApplyVendor(byName[name], values);
                updated++;
            }
            else
            {
                var vendor = new Vendor { Name = name, CreatedAt = DateTime.UtcNow };
                ApplyVendor(vendor, values);
                db.Vendors.Add(vendor);
                inserted++;
            }
        }

        await db.SaveChangesAsync(ct);
        return new ImportResultDto("Locations", inserted, updated, skipped, invalid, []);
    }

    private static (ImportRowStatus, string?) ValidateVendorRow(
        IReadOnlyDictionary<string, string?> v, HashSet<string> existing, HashSet<string> seen)
    {
        var name = v["Name"]?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return (ImportRowStatus.Invalid, "Name is required.");

        if (!seen.Add(name))
            return (ImportRowStatus.Duplicate, "Name appears more than once in this file.");
        if (existing.Contains(name))
            return (ImportRowStatus.Duplicate, "Location already exists in the database.");

        return (ImportRowStatus.New, null);
    }

    private static void ApplyVendor(Vendor x, IReadOnlyDictionary<string, string?> v)
    {
        x.Name = v["Name"]!.Trim();
        x.IsOwnCompany = ParseBool(v["IsOwnCompany"]);
        x.StoresStock = ParseBool(v["StoresStock"]);
        x.DoesBlasting = ParseBool(v["DoesBlasting"]);
        x.DoesPainting = ParseBool(v["DoesPainting"]);
    }

    // ---------- Shared helpers ----------

    private static ImportPreviewDto Summarise(string entity, string[] columns, List<ImportRowDto> rows) =>
        new(entity, columns, rows.Count,
            rows.Count(r => r.Status == ImportRowStatus.New),
            rows.Count(r => r.Status == ImportRowStatus.Duplicate),
            rows.Count(r => r.Status == ImportRowStatus.Invalid),
            rows);

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static decimal? ParseDecimal(string? s) =>
        decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;

    private static bool ParseBool(string? s) =>
        s?.Trim().ToLowerInvariant() is "true" or "1" or "yes" or "y";

    private static byte[] Write(IReadOnlyList<string> header, IReadOnlyList<string[]> rows)
    {
        var sb = new StringBuilder();
        sb.Append('\uFEFF'); // BOM so Excel opens UTF-8 correctly
        sb.AppendLine(string.Join(',', header.Select(Escape)));
        foreach (var row in rows)
            sb.AppendLine(string.Join(',', row.Select(Escape)));
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    // Reads a CSV stream into rows keyed by the requested columns (header-name matched, case-insensitive).
    private static List<Dictionary<string, string?>> Read(Stream stream, string[] expected)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = reader.ReadToEnd();
        var records = ParseCsv(text);
        if (records.Count == 0)
            throw new CsvFormatException("The file is empty.");

        var header = records[0];
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Count; i++)
            map[header[i].Trim()] = i;

        var missing = expected.Where(c => !map.ContainsKey(c)).ToList();
        if (missing.Count > 0)
            throw new CsvFormatException($"Missing required column(s): {string.Join(", ", missing)}.");

        var table = new List<Dictionary<string, string?>>();
        for (var r = 1; r < records.Count; r++)
        {
            var fields = records[r];
            if (fields.Count == 1 && string.IsNullOrWhiteSpace(fields[0])) continue; // blank line
            var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var col in expected)
                row[col] = map[col] < fields.Count ? fields[map[col]] : null;
            table.Add(row);
        }

        return table;
    }

    // Minimal RFC-4180 parser: quoted fields, escaped quotes, embedded commas/newlines.
    private static List<List<string>> ParseCsv(string text)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(ch);
            }
            else
            {
                switch (ch)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        record.Add(field.ToString());
                        field.Clear();
                        break;
                    case '\r':
                        break;
                    case '\n':
                        record.Add(field.ToString());
                        field.Clear();
                        records.Add(record);
                        record = [];
                        break;
                    default:
                        field.Append(ch);
                        break;
                }
            }
        }

        if (field.Length > 0 || record.Count > 0)
        {
            record.Add(field.ToString());
            records.Add(record);
        }

        return records;
    }
}

public sealed class CsvFormatException(string message) : Exception(message);