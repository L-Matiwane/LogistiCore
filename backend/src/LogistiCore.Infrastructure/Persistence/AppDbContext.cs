using LogistiCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogistiCore.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<RateCard> RateCards => Set<RateCard>();
    public DbSet<Parcel> Parcels => Set<Parcel>();
    public DbSet<ParcelStatusHistory> StatusHistory => Set<ParcelStatusHistory>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("Customers");
            e.HasKey(x => x.CustomerId);
        });

        modelBuilder.Entity<RateCard>(e =>
        {
            e.ToTable("RateCards");
            e.HasKey(x => x.RateCardId);
            e.Property(x => x.MinWeightKg).HasPrecision(8, 2);
            e.Property(x => x.MaxWeightKg).HasPrecision(8, 2);
            e.Property(x => x.Price).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Parcel>(e =>
        {
            e.ToTable("Parcels");
            e.HasKey(x => x.ParcelId);
            e.HasIndex(x => x.TrackingNo).IsUnique();
            e.Property(x => x.WeightKg).HasPrecision(8, 2);
            e.Property(x => x.LengthCm).HasPrecision(8, 2);
            e.Property(x => x.WidthCm).HasPrecision(8, 2);
            e.Property(x => x.HeightCm).HasPrecision(8, 2);
            e.Property(x => x.BilledWeightKg).HasPrecision(8, 2);
            e.Property(x => x.Price).HasPrecision(10, 2);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
            e.HasMany(x => x.History).WithOne().HasForeignKey(h => h.ParcelId);
        });

        modelBuilder.Entity<ParcelStatusHistory>(e =>
        {
            e.ToTable("ParcelStatusHistory");
            e.HasKey(x => x.HistoryId);
        });
    }
}
