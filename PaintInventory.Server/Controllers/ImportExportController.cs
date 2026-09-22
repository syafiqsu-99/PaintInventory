using Microsoft.AspNetCore.Mvc;
using PaintInventory.Server.Models;
using PaintInventory.Server.Services;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/importexport")]
public sealed class ImportExportController(CsvService csv) : ControllerBase
{
    private const string CsvType = "text/csv";

    [HttpGet("products/export")]
    public async Task<IActionResult> ExportProducts(CancellationToken ct)
        => File(await csv.ExportProductsAsync(ct), CsvType, $"products-{Stamp()}.csv");

    [HttpGet("vendors/export")]
    public async Task<IActionResult> ExportVendors(CancellationToken ct)
        => File(await csv.ExportVendorsAsync(ct), CsvType, $"locations-{Stamp()}.csv");

    [HttpGet("products/template")]
    public IActionResult ProductTemplate()
        => File(csv.Template(CsvService.ProductColumns), CsvType, "products-template.csv");

    [HttpGet("vendors/template")]
    public IActionResult VendorTemplate()
        => File(csv.Template(CsvService.VendorColumns), CsvType, "locations-template.csv");

    [HttpPost("products/preview")]
    public async Task<ActionResult<ImportPreviewDto>> PreviewProducts(IFormFile file, CancellationToken ct)
    {
        if (!Valid(file, out var error)) return BadRequest(new { error });
        try
        {
            await using var stream = file.OpenReadStream();
            return Ok(await csv.PreviewProductsAsync(stream, ct));
        }
        catch (CsvFormatException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("products/commit")]
    public async Task<ActionResult<ImportResultDto>> CommitProducts(
        IFormFile file, [FromForm] ImportMode mode, CancellationToken ct)
    {
        if (!Valid(file, out var error)) return BadRequest(new { error });
        try
        {
            await using var stream = file.OpenReadStream();
            return Ok(await csv.CommitProductsAsync(stream, mode, ct));
        }
        catch (CsvFormatException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("vendors/preview")]
    public async Task<ActionResult<ImportPreviewDto>> PreviewVendors(IFormFile file, CancellationToken ct)
    {
        if (!Valid(file, out var error)) return BadRequest(new { error });
        try
        {
            await using var stream = file.OpenReadStream();
            return Ok(await csv.PreviewVendorsAsync(stream, ct));
        }
        catch (CsvFormatException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("vendors/commit")]
    public async Task<ActionResult<ImportResultDto>> CommitVendors(
        IFormFile file, [FromForm] ImportMode mode, CancellationToken ct)
    {
        if (!Valid(file, out var error)) return BadRequest(new { error });
        try
        {
            await using var stream = file.OpenReadStream();
            return Ok(await csv.CommitVendorsAsync(stream, mode, ct));
        }
        catch (CsvFormatException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static bool Valid(IFormFile? file, out string error)
    {
        if (file is null || file.Length == 0) { error = "Choose a CSV file to upload."; return false; }
        if (file.Length > 5 * 1024 * 1024) { error = "File is larger than the 5 MB limit."; return false; }
        error = string.Empty;
        return true;
    }

    private static string Stamp() => DateTime.UtcNow.ToString("yyyyMMdd-HHmm");
}