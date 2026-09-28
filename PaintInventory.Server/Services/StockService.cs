using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;
using PaintInventory.Server.Infrastructure;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Services;

public enum StockOpStatus
{
    Ok,
    ProductNotFound,
    LocationNotFound,
    TransactionNotFound,
    InsufficientStock,
    Invalid
}

public sealed record StockOutcome(
    StockOpStatus Status,
    StockResult? Result = null,
    StockResult? Secondary = null,
    string? Error = null);

public sealed class StockService(PaintInventoryDbContext db, UserContext user)
{
    public async Task<StockOutcome> StockInAsync(StockInRequest req, CancellationToken ct)
    {
        if (req.Quantity <= 0)
            return Invalid("Quantity must be greater than zero.");

        var product = await ActiveProductAsync(req.ProductId, ct);
        if (product is null)
            return new StockOutcome(StockOpStatus.ProductNotFound, Error: $"Product {req.ProductId} not found.");

        var vendor = await ActiveVendorAsync(req.VendorId, ct);
        if (vendor is null)
            return new StockOutcome(StockOpStatus.LocationNotFound, Error: $"Location {req.VendorId} not found.");

        var operatorName = OperatorFor(req.Operator);
        var now = DateTime.UtcNow;
        var balance = await GetOrCreateBalanceAsync(req.ProductId, req.VendorId, ct);
        balance.OnHandQty += req.Quantity;
        balance.UpdatedAt = now;

        var txn = new StockTransaction
        {
            PaintProductId = req.ProductId,
            VendorId = req.VendorId,
            Direction = StockDirection.In,
            Quantity = req.Quantity,
            Batch = req.Batch,
            Shade = req.Shade,
            PackVolume = req.PackVolume ?? product.PackVolume,
            ManufacturingDate = req.ManufacturingDate,
            BestBefore = req.BestBefore,
            Source = req.Source,
            Operator = operatorName,
            Notes = req.Notes,
            DeviceId = req.DeviceId,
            Timestamp = now
        };
        db.StockTransactions.Add(txn);
        AddAudit(nameof(StockTransaction), StockDirection.In.ToString(), operatorName,
            new { req.ProductId, req.VendorId, req.Quantity, newOnHand = balance.OnHandQty, scanRaw = req.ScanRaw });

        await SaveWithRetryAsync(ct);
        return Success(txn, balance, product, vendor, req.Quantity);
    }

    public async Task<StockOutcome> StockOutAsync(StockOutRequest req, CancellationToken ct)
    {
        if (!user.IsStaff)
        {
            if (user.VendorId is not int ownVendorId)
                return Invalid("Your session has no site assigned.");
            req = req with { VendorId = ownVendorId };
        }

        if (req.Quantity <= 0)
            return Invalid("Quantity must be greater than zero.");

        var product = await ActiveProductAsync(req.ProductId, ct);
        if (product is null)
            return new StockOutcome(StockOpStatus.ProductNotFound, Error: $"Product {req.ProductId} not found.");

        var vendor = await ActiveVendorAsync(req.VendorId, ct);
        if (vendor is null)
            return new StockOutcome(StockOpStatus.LocationNotFound, Error: $"Location {req.VendorId} not found.");

        var balance = await db.StockBalances
            .FirstOrDefaultAsync(b => b.PaintProductId == req.ProductId && b.VendorId == req.VendorId, ct);
        if (balance is null || balance.OnHandQty < req.Quantity)
            return new StockOutcome(StockOpStatus.InsufficientStock,
                Error: $"Insufficient stock at {vendor.Name}. On hand: {balance?.OnHandQty ?? 0m}, requested: {req.Quantity}.");

        var operatorName = OperatorFor(req.Operator);
        var now = DateTime.UtcNow;
        balance.OnHandQty -= req.Quantity;
        balance.UpdatedAt = now;

        var txn = new StockTransaction
        {
            PaintProductId = req.ProductId,
            VendorId = req.VendorId,
            Direction = StockDirection.Out,
            Quantity = req.Quantity,
            Batch = req.Batch,
            Shade = req.Shade,
            CoatLineId = req.CoatLineId,
            Operator = operatorName,
            Notes = req.Notes,
            DeviceId = req.DeviceId,
            Timestamp = now
        };
        db.StockTransactions.Add(txn);
        AddAudit(nameof(StockTransaction), StockDirection.Out.ToString(), operatorName,
            new { req.ProductId, req.VendorId, req.Quantity, req.CoatLineId, newOnHand = balance.OnHandQty, scanRaw = req.ScanRaw });

        await SaveWithRetryAsync(ct);
        return Success(txn, balance, product, vendor, -req.Quantity);
    }

    public async Task<StockOutcome> AdjustAsync(StockAdjustRequest req, CancellationToken ct)
    {
        if (req.NewOnHandQty < 0)
            return Invalid("On-hand quantity cannot be negative.");

        var product = await ActiveProductAsync(req.ProductId, ct);
        if (product is null)
            return new StockOutcome(StockOpStatus.ProductNotFound, Error: $"Product {req.ProductId} not found.");

        var vendor = await ActiveVendorAsync(req.VendorId, ct);
        if (vendor is null)
            return new StockOutcome(StockOpStatus.LocationNotFound, Error: $"Location {req.VendorId} not found.");

        var operatorName = OperatorFor(req.Operator);
        var now = DateTime.UtcNow;
        var balance = await GetOrCreateBalanceAsync(req.ProductId, req.VendorId, ct);
        var previous = balance.OnHandQty;
        var delta = req.NewOnHandQty - previous;
        balance.OnHandQty = req.NewOnHandQty;
        balance.UpdatedAt = now;

        var txn = new StockTransaction
        {
            PaintProductId = req.ProductId,
            VendorId = req.VendorId,
            Direction = StockDirection.Adjustment,
            Quantity = Math.Abs(delta),
            Operator = operatorName,
            Notes = req.Notes,
            Timestamp = now
        };
        db.StockTransactions.Add(txn);
        AddAudit(nameof(StockTransaction), StockDirection.Adjustment.ToString(), operatorName,
            new { req.ProductId, req.VendorId, previous, adjustedTo = req.NewOnHandQty, delta });

        await SaveWithRetryAsync(ct);
        return Success(txn, balance, product, vendor, delta);
    }

    public async Task<StockOutcome> TransferAsync(StockTransferRequest req, CancellationToken ct)
    {
        if (req.Quantity <= 0)
            return Invalid("Quantity must be greater than zero.");
        if (req.FromVendorId == req.ToVendorId)
            return Invalid("Source and destination locations must differ.");

        var product = await ActiveProductAsync(req.ProductId, ct);
        if (product is null)
            return new StockOutcome(StockOpStatus.ProductNotFound, Error: $"Product {req.ProductId} not found.");

        var from = await ActiveVendorAsync(req.FromVendorId, ct);
        var to = await ActiveVendorAsync(req.ToVendorId, ct);
        if (from is null || to is null)
            return new StockOutcome(StockOpStatus.LocationNotFound, Error: "Source or destination location not found.");

        var fromBalance = await db.StockBalances
            .FirstOrDefaultAsync(b => b.PaintProductId == req.ProductId && b.VendorId == req.FromVendorId, ct);
        if (fromBalance is null || fromBalance.OnHandQty < req.Quantity)
            return new StockOutcome(StockOpStatus.InsufficientStock,
                Error: $"Insufficient stock at {from.Name}. On hand: {fromBalance?.OnHandQty ?? 0m}, requested: {req.Quantity}.");

        var operatorName = OperatorFor(req.Operator);
        var now = DateTime.UtcNow;
        var inTransit = !to.IsOwnCompany;

        fromBalance.OnHandQty -= req.Quantity;
        fromBalance.UpdatedAt = now;

        StockBalance toBalance;
        if (inTransit)
        {
            var existing = await db.StockBalances.AsNoTracking()
                .Where(b => b.PaintProductId == req.ProductId && b.VendorId == req.ToVendorId)
                .Select(b => new { b.OnHandQty, b.ReorderLevel })
                .FirstOrDefaultAsync(ct);
            toBalance = new StockBalance { OnHandQty = existing?.OnHandQty ?? 0m, ReorderLevel = existing?.ReorderLevel };
        }
        else
        {
            toBalance = await GetOrCreateBalanceAsync(req.ProductId, req.ToVendorId, ct);
            toBalance.OnHandQty += req.Quantity;
            toBalance.UpdatedAt = now;
        }

        var txn = new StockTransaction
        {
            PaintProductId = req.ProductId,
            VendorId = req.FromVendorId,
            CounterpartyVendorId = req.ToVendorId,
            Direction = StockDirection.Transfer,
            Quantity = req.Quantity,
            Batch = req.Batch,
            Operator = operatorName,
            Notes = req.Notes,
            Timestamp = now,
            ReceivedAt = inTransit ? null : now,
            ReceivedBy = inTransit ? null : operatorName,
            ReceivedQty = inTransit ? null : req.Quantity
        };
        db.StockTransactions.Add(txn);
        AddAudit(nameof(StockTransaction), StockDirection.Transfer.ToString(), operatorName,
            new { req.ProductId, req.FromVendorId, req.ToVendorId, req.Quantity, inTransit });

        await SaveWithRetryAsync(ct);

        var fromResult = BuildResult(txn.Id, product, from, StockDirection.Transfer, -req.Quantity, fromBalance, now);
        var toResult = BuildResult(txn.Id, product, to, StockDirection.Transfer, inTransit ? 0m : req.Quantity, toBalance, now);
        return new StockOutcome(StockOpStatus.Ok, fromResult, toResult);
    }

    public async Task<StockOutcome> ReceiveAsync(StockReceiveRequest req, CancellationToken ct)
    {
        if (req.ReceivedQty <= 0)
            return Invalid("Received quantity must be greater than zero.");

        var txn = await db.StockTransactions
            .FirstOrDefaultAsync(t => t.Id == req.TransactionId
                && t.Direction == StockDirection.Transfer
                && t.ReceivedAt == null, ct);
        if (txn is null || txn.CounterpartyVendorId is not int toVendorId)
            return new StockOutcome(StockOpStatus.TransactionNotFound,
                Error: $"No pending transfer {req.TransactionId} to receive.");

        if (!user.IsStaff && user.VendorId != toVendorId)
            return new StockOutcome(StockOpStatus.TransactionNotFound,
                Error: $"No pending transfer {req.TransactionId} to receive.");

        if (req.ReceivedQty > txn.Quantity)
            return Invalid($"Received quantity cannot exceed the {txn.Quantity} sent.");

        var product = await db.PaintProducts.FirstAsync(p => p.Id == txn.PaintProductId, ct);
        var vendor = await ActiveVendorAsync(toVendorId, ct);
        if (vendor is null)
            return new StockOutcome(StockOpStatus.LocationNotFound, Error: $"Location {toVendorId} not found.");

        await using var dbTxn = await db.Database.BeginTransactionAsync(ct);

        var now = DateTime.UtcNow;
        var operatorName = OperatorFor(null);

        var claimed = await db.StockTransactions
            .Where(t => t.Id == txn.Id && t.ReceivedAt == null)
            .ExecuteUpdateAsync(u => u
                .SetProperty(t => t.ReceivedAt, now)
                .SetProperty(t => t.ReceivedBy, operatorName)
                .SetProperty(t => t.ReceivedQty, req.ReceivedQty), ct);
        if (claimed == 0)
            return new StockOutcome(StockOpStatus.TransactionNotFound,
                Error: $"Transfer {req.TransactionId} was already received.");

        var balance = await GetOrCreateBalanceAsync(txn.PaintProductId, toVendorId, ct);
        balance.OnHandQty += req.ReceivedQty;
        balance.UpdatedAt = now;

        var receivedBatch = string.IsNullOrWhiteSpace(req.Batch) ? null : req.Batch.Trim();
        if (txn.Batch is null && receivedBatch is not null)
            txn.Batch = receivedBatch;
        if (!string.IsNullOrWhiteSpace(req.Notes))
            txn.Notes = string.IsNullOrWhiteSpace(txn.Notes) ? req.Notes : $"{txn.Notes} | Received: {req.Notes}";

        var variance = req.ReceivedQty - txn.Quantity;
        AddAudit(nameof(StockTransaction), variance == 0 ? "Received" : "ReceivedWithVariance", operatorName,
            new
            {
                transactionId = txn.Id,
                txn.PaintProductId,
                toVendorId,
                sent = txn.Quantity,
                received = req.ReceivedQty,
                variance,
                sentBatch = txn.Batch,
                receivedBatch,
                newOnHand = balance.OnHandQty,
                scanRaw = req.ScanRaw
            });

        await SaveWithRetryAsync(ct);
        await dbTxn.CommitAsync(ct);

        var isLow = balance.ReorderLevel.HasValue && balance.OnHandQty <= balance.ReorderLevel.Value;
        return new StockOutcome(StockOpStatus.Ok, new StockResult(txn.Id, product.Id, product.Gtin, vendor.Id, vendor.Name,
            StockDirection.Transfer, req.ReceivedQty, balance.OnHandQty, isLow, now));
    }

    public Task<List<InTransitDto>> GetInTransitAsync(int? toVendorId, CancellationToken ct)
    {
        if (!user.IsStaff)
            toVendorId = user.VendorId ?? -1;

        var q = db.StockTransactions.AsNoTracking()
            .Where(t => t.Direction == StockDirection.Transfer && t.ReceivedAt == null && t.CounterpartyVendorId != null);
        if (toVendorId is not null)
            q = q.Where(t => t.CounterpartyVendorId == toVendorId);

        return q.OrderBy(t => t.Timestamp)
            .Select(t => new InTransitDto(
                t.Id, t.PaintProductId, t.PaintProduct.Gtin, t.PaintProduct.ProductName, t.PaintProduct.Component,
                t.Quantity, t.Batch, t.VendorId, t.Vendor.Name,
                t.CounterpartyVendorId!.Value, t.CounterpartyVendor!.Name, t.Operator, t.Timestamp))
            .ToListAsync(ct);
    }

    private string? OperatorFor(string? requested) =>
        string.IsNullOrWhiteSpace(user.Operator) ? requested : user.Operator;

    private Task<PaintProduct?> ActiveProductAsync(int id, CancellationToken ct) =>
        db.PaintProducts.FirstOrDefaultAsync(p => p.Id == id && p.IsActive, ct);

    private Task<Vendor?> ActiveVendorAsync(int id, CancellationToken ct) =>
        db.Vendors.FirstOrDefaultAsync(v => v.Id == id && v.IsActive, ct);

    private async Task<StockBalance> GetOrCreateBalanceAsync(int productId, int vendorId, CancellationToken ct)
    {
        var balance = await db.StockBalances
            .FirstOrDefaultAsync(b => b.PaintProductId == productId && b.VendorId == vendorId, ct);
        if (balance is null)
        {
            balance = new StockBalance
            {
                PaintProductId = productId,
                VendorId = vendorId,
                OnHandQty = 0m,
                UpdatedAt = DateTime.UtcNow
            };
            db.StockBalances.Add(balance);
        }
        return balance;
    }

    private void AddAudit(string entity, string action, string? by, object details) =>
        db.AuditLogs.Add(new AuditLog
        {
            Entity = entity,
            Action = action,
            ChangedBy = by,
            ChangedAt = DateTime.UtcNow,
            Details = JsonSerializer.Serialize(details)
        });

    private async Task SaveWithRetryAsync(CancellationToken ct, int maxAttempts = 3)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.SaveChangesAsync(ct);
                return;
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < maxAttempts)
            {
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is not StockBalance) continue;

                    var dbValues = await entry.GetDatabaseValuesAsync(ct);
                    if (dbValues is null) continue;

                    var original = entry.OriginalValues.GetValue<decimal>(nameof(StockBalance.OnHandQty));
                    var proposed = entry.CurrentValues.GetValue<decimal>(nameof(StockBalance.OnHandQty));
                    var dbOnHand = dbValues.GetValue<decimal>(nameof(StockBalance.OnHandQty));

                    entry.OriginalValues.SetValues(dbValues);
                    entry.CurrentValues[nameof(StockBalance.OnHandQty)] = dbOnHand + (proposed - original);
                }
            }
        }
    }

    private static StockOutcome Success(
        StockTransaction txn, StockBalance balance, PaintProduct product, Vendor vendor, decimal applied)
    {
        var result = BuildResult(txn.Id, product, vendor, txn.Direction, applied, balance, txn.Timestamp);
        return new StockOutcome(StockOpStatus.Ok, result);
    }

    private static StockResult BuildResult(
        int txnId, PaintProduct product, Vendor vendor, StockDirection direction,
        decimal applied, StockBalance balance, DateTime timestamp)
    {
        var isLow = balance.ReorderLevel.HasValue && balance.OnHandQty <= balance.ReorderLevel.Value;
        return new StockResult(txnId, product.Id, product.Gtin, vendor.Id, vendor.Name,
            direction, applied, balance.OnHandQty, isLow, timestamp);
    }

    private static StockOutcome Invalid(string message) => new(StockOpStatus.Invalid, Error: message);
}