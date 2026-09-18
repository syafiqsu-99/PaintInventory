using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Data;

public class PaintInventoryDbContext(DbContextOptions<PaintInventoryDbContext> options)
    : DbContext(options)
{
    public DbSet<PaintItem> PaintItems => Set<PaintItem>();
    public DbSet<ScanRecord> ScanRecords => Set<ScanRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LocationHistory> LocationHistories => Set<LocationHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaintItem>()
            .HasIndex(p => p.Barcode)
            .IsUnique();

        modelBuilder.Entity<PaintItem>()
            .Property(p => p.Volume)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PaintItem>()
            .Property(p => p.OnHand)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PaintItem>()
            .Property(p => p.ReorderLevel)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ScanRecord>()
            .Property(s => s.Quantity)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ScanRecord>()
            .HasIndex(s => s.Timestamp);

        modelBuilder.Entity<LocationHistory>()
            .Property(l => l.QuantityMoved)
            .HasPrecision(18, 2);

        modelBuilder.Entity<LocationHistory>()
            .HasOne(l => l.PaintItem)
            .WithMany(p => p.LocationHistories)
            .HasForeignKey(l => l.PaintItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScanRecord>()
            .HasOne(s => s.PaintItem)
            .WithMany(p => p.ScanRecords)
            .HasForeignKey(s => s.PaintItemId)
            .OnDelete(DeleteBehavior.SetNull);

        base.OnModelCreating(modelBuilder);
    }
}