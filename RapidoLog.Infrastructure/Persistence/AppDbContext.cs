using Microsoft.EntityFrameworkCore;
using RapidoLog.Application.Common.Interfaces;
using RapidoLog.Domain.Entities;

namespace RapidoLog.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shipment>()
            .Property(s => s.TrackingNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        modelBuilder.Entity<PaymentTransaction>()
            .Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");
    }
}