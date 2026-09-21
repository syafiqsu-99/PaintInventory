using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Models;

namespace PaintInventory.Server.Data;

public class PaintInventoryDbContext(DbContextOptions<PaintInventoryDbContext> options)
    : DbContext(options)
{
    public DbSet<PaintProduct> PaintProducts => Set<PaintProduct>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<PaintReport> PaintReports => Set<PaintReport>();
    public DbSet<PaintReportItem> PaintReportItems => Set<PaintReportItem>();
    public DbSet<SurfacePrep> SurfacePreps => Set<SurfacePrep>();
    public DbSet<CoatLine> CoatLines => Set<CoatLine>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaintProduct>().ToTable("PaintInventory_Products");
        modelBuilder.Entity<Vendor>().ToTable("PaintInventory_Vendors");
        modelBuilder.Entity<StockBalance>().ToTable("PaintInventory_StockBalances");
        modelBuilder.Entity<StockTransaction>().ToTable("PaintInventory_StockTransactions");
        modelBuilder.Entity<PaintReport>().ToTable("PaintInventory_Reports");
        modelBuilder.Entity<PaintReportItem>().ToTable("PaintInventory_ReportItems");
        modelBuilder.Entity<SurfacePrep>().ToTable("PaintInventory_SurfacePreps");
        modelBuilder.Entity<CoatLine>().ToTable("PaintInventory_CoatLines");
        modelBuilder.Entity<AuditLog>().ToTable("PaintInventory_AuditLogs");

        modelBuilder.Entity<PaintProduct>(e =>
        {
            e.HasIndex(p => p.Gtin).IsUnique();
            e.Property(p => p.PackVolume).HasPrecision(18, 2);

            e.HasOne(p => p.PartnerProduct)
                .WithMany()
                .HasForeignKey(p => p.PartnerProductId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Vendor>(e =>
        {
            e.HasIndex(v => v.Name).IsUnique();
        });

        modelBuilder.Entity<StockBalance>(e =>
        {
            e.HasIndex(b => new { b.PaintProductId, b.VendorId }).IsUnique();
            e.Property(b => b.OnHandQty).HasPrecision(18, 2);
            e.Property(b => b.ReorderLevel).HasPrecision(18, 2);
            e.Property(b => b.RowVersion).IsRowVersion();

            e.HasOne(b => b.PaintProduct)
                .WithMany(p => p.StockBalances)
                .HasForeignKey(b => b.PaintProductId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.Vendor)
                .WithMany(v => v.StockBalances)
                .HasForeignKey(b => b.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockTransaction>(e =>
        {
            e.HasIndex(t => t.Timestamp);
            e.HasIndex(t => t.PaintProductId);
            e.HasIndex(t => t.VendorId);
            e.Property(t => t.Quantity).HasPrecision(18, 2);
            e.Property(t => t.PackVolume).HasPrecision(18, 2);

            e.HasOne(t => t.PaintProduct)
                .WithMany(p => p.StockTransactions)
                .HasForeignKey(t => t.PaintProductId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Vendor)
                .WithMany()
                .HasForeignKey(t => t.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.CounterpartyVendor)
                .WithMany()
                .HasForeignKey(t => t.CounterpartyVendorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.CoatLine)
                .WithMany(c => c.StockTransactions)
                .HasForeignKey(t => t.CoatLineId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PaintReport>(e =>
        {
            e.HasIndex(r => r.Ipo);
        });

        modelBuilder.Entity<PaintReportItem>(e =>
        {
            e.HasIndex(i => new { i.PaintReportId, i.ItemNo });
            e.Property(i => i.RequiredTotalDftUm).HasPrecision(9, 2);
            e.Property(i => i.MeasuredTotalDftUm).HasPrecision(9, 2);

            e.HasOne(i => i.Report)
                .WithMany(r => r.Items)
                .HasForeignKey(i => i.PaintReportId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.BlastVendor)
                .WithMany()
                .HasForeignKey(i => i.BlastVendorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.PaintingVendor)
                .WithMany()
                .HasForeignKey(i => i.PaintingVendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SurfacePrep>(e =>
        {
            e.HasIndex(s => s.PaintReportItemId).IsUnique();
            e.Property(s => s.MeasuredRoughness).HasPrecision(9, 2);
            e.Property(s => s.HumidityPct).HasPrecision(6, 2);
            e.Property(s => s.AirTempC).HasPrecision(6, 2);
            e.Property(s => s.SubstrateTempC).HasPrecision(6, 2);
            e.Property(s => s.DewPointC).HasPrecision(6, 2);

            e.HasOne(s => s.ReportItem)
                .WithOne(i => i.SurfacePrep)
                .HasForeignKey<SurfacePrep>(s => s.PaintReportItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CoatLine>(e =>
        {
            e.HasIndex(c => new { c.PaintReportItemId, c.Sequence });
            e.Property(c => c.RequiredThicknessUm).HasPrecision(9, 2);
            e.Property(c => c.MeasuredThicknessUm).HasPrecision(9, 2);
            e.Property(c => c.HumidityPct).HasPrecision(6, 2);
            e.Property(c => c.AirTempC).HasPrecision(6, 2);
            e.Property(c => c.SubstrateTempC).HasPrecision(6, 2);
            e.Property(c => c.DewPointC).HasPrecision(6, 2);
            e.Property(c => c.PartAQtyUsed).HasPrecision(18, 2);
            e.Property(c => c.PartBQtyUsed).HasPrecision(18, 2);

            e.HasOne(c => c.ReportItem)
                .WithMany(i => i.Coats)
                .HasForeignKey(c => c.PaintReportItemId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.PartAProduct)
                .WithMany()
                .HasForeignKey(c => c.PartAProductId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.PartBProduct)
                .WithMany()
                .HasForeignKey(c => c.PartBProductId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.StockLocationVendor)
                .WithMany()
                .HasForeignKey(c => c.StockLocationVendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}