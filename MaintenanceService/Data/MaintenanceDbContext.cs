using Microsoft.EntityFrameworkCore;
using MaintenanceService.Models;

public class MaintenanceDbContext : DbContext
{
    public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options) : base(options) { }

    public DbSet<Maintenance> Maintenances { get; set; } = null!;
    public DbSet<SparePart> SpareParts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Maintenance entity
        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.VehicleId)
                .IsRequired();

            entity.Property(e => e.MaintenanceType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Priority)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.MechanicNotes)
                .HasMaxLength(1000);

            entity.Property(e => e.EstimatedCost)
                .HasPrecision(10, 2);

            entity.Property(e => e.ActualCost)
                .HasPrecision(10, 2);

            entity.HasMany(e => e.SpareParts)
                .WithOne(sp => sp.Maintenance)
                .HasForeignKey(sp => sp.MaintenanceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledDate);
        });

        // Configure SparePart entity
        modelBuilder.Entity<SparePart>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.MaintenanceId)
                .IsRequired();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.PartNumber)
                .HasMaxLength(100);

            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2);

            entity.Property(e => e.TotalPrice)
                .HasPrecision(10, 2);

            entity.HasIndex(e => e.MaintenanceId);
        });
    }
}
