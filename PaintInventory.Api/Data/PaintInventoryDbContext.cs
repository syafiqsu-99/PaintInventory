using Microsoft.EntityFrameworkCore;
using PaintInventory.Api.Models;

namespace PaintInventory.Api.Data;

public class PaintInventoryDbContext : DbContext
{
    public PaintInventoryDbContext(DbContextOptions<PaintInventoryDbContext> options) : base(options)
    {
    }

    public DbSet<PaintItem> PaintItems => Set<PaintItem>();
    public DbSet<ScanRecord> ScanRecords => Set<ScanRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LocationHistory> LocationHistories => Set<LocationHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaintItem>()
            .HasIndex(p => p.Barcode)
            .IsUnique();

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
