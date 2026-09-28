using Microsoft.AspNetCore.Mvc;
using PaintInventory.Server.Models;
using PaintInventory.Server.Services;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StockController(StockService stock) : ControllerBase
{
    [HttpPost("in")]
    public async Task<ActionResult<StockResult>> StockIn(StockInRequest req, CancellationToken ct)
        => Map(await stock.StockInAsync(req, ct));

    [HttpPost("out")]
    public async Task<ActionResult<StockResult>> StockOut(StockOutRequest req, CancellationToken ct)
        => Map(await stock.StockOutAsync(req, ct));

    [HttpPost("adjust")]
    public async Task<ActionResult<StockResult>> Adjust(StockAdjustRequest req, CancellationToken ct)
        => Map(await stock.AdjustAsync(req, ct));

    [HttpPost("transfer")]
    public async Task<ActionResult<TransferResult>> Transfer(StockTransferRequest req, CancellationToken ct)
    {
        var outcome = await stock.TransferAsync(req, ct);
        return outcome.Status switch
        {
            StockOpStatus.Ok => Ok(new TransferResult(outcome.Result!, outcome.Secondary!)),
            StockOpStatus.ProductNotFound or StockOpStatus.LocationNotFound => NotFound(new { error = outcome.Error }),
            StockOpStatus.InsufficientStock => Conflict(new { error = outcome.Error }),
            _ => BadRequest(new { error = outcome.Error })
        };
    }

    private ActionResult<StockResult> Map(StockOutcome outcome) => outcome.Status switch
    {
        StockOpStatus.Ok => Ok(outcome.Result),
        StockOpStatus.ProductNotFound or StockOpStatus.LocationNotFound => NotFound(new { error = outcome.Error }),
        StockOpStatus.InsufficientStock => Conflict(new { error = outcome.Error }),
        _ => BadRequest(new { error = outcome.Error })
    };
}