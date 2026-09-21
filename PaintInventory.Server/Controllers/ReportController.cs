using Microsoft.AspNetCore.Mvc;
using PaintInventory.Server.Models;
using PaintInventory.Server.Services;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController(ReportService reports) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportListItemDto>>> GetAll(CancellationToken ct)
        => Ok(await reports.ListAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReportDto>> GetById(int id, CancellationToken ct)
    {
        var dto = await reports.GetAsync(id, ct);
        return dto is null ? NotFound(new { error = $"Report {id} not found." }) : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ReportSaveResult>> Create(ReportRequest req, CancellationToken ct)
    {
        if (req.Items is null || req.Items.Count == 0)
            return BadRequest(new { error = "A report needs at least one item." });

        var result = await reports.CreateAsync(req, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Report.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReportSaveResult>> Update(int id, ReportRequest req, CancellationToken ct)
    {
        if (req.Items is null || req.Items.Count == 0)
            return BadRequest(new { error = "A report needs at least one item." });

        var result = await reports.UpdateAsync(id, req, ct);
        return result is null ? NotFound(new { error = $"Report {id} not found." }) : Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await reports.DeleteAsync(id, ct)
            ? NoContent()
            : NotFound(new { error = $"Report {id} not found." });
}