using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintInventory.Server.Infrastructure;
using PaintInventory.Server.Models;
using PaintInventory.Server.Services;

namespace PaintInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StockController(StockService stock) : ControllerBase
{
    [Authorize(Policy = AuthConstants.StaffPolicy)]
    [HttpPost("in")]
    public async Task<ActionResult<StockResult>> StockIn(StockInRequest req, CancellationToken ct)
        => Map(await stock.StockInAsync(req, ct));

    [HttpPost("out")]
    public async Task<ActionResult<StockResult>> StockOut(StockOutRequest req, CancellationToken ct)
        => Map(await stock.StockOutAsync(req, ct));

    [Authorize(Policy = AuthConstants.StaffPolicy)]
    [HttpPost("adjust")]
    public async Task<ActionResult<StockResult>> Adjust(StockAdjustRequest req, CancellationToken ct)
        => Map(await stock.AdjustAsync(req, ct));

    [Authorize(Policy = AuthConstants.StaffPolicy)]
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

    [HttpPost("receive")]
    public async Task<ActionResult<StockResult>> Receive(StockReceiveRequest req, CancellationToken ct)
        => Map(await stock.ReceiveAsync(req, ct));

    [HttpGet("in-transit")]
    public async Task<ActionResult<IEnumerable<InTransitDto>>> InTransit([FromQuery] int? vendorId, CancellationToken ct)
        => Ok(await stock.GetInTransitAsync(vendorId, ct));

    private ActionResult<StockResult> Map(StockOutcome outcome) => outcome.Status switch
    {
        StockOpStatus.Ok => Ok(outcome.Result),
        StockOpStatus.ProductNotFound or StockOpStatus.LocationNotFound or StockOpStatus.TransactionNotFound
            => NotFound(new { error = outcome.Error }),
        StockOpStatus.InsufficientStock => Conflict(new { error = outcome.Error }),
        _ => BadRequest(new { error = outcome.Error })
    };
}