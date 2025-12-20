using Microsoft.EntityFrameworkCore;
using QL_HethongDiennuoc.Models.Entities;

namespace QL_HethongDiennuoc.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Meter> Meters { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<Bill> Bills { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tariff> Tariffs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Meter>()
            .HasOne(m => m.Customer)
            .WithMany(c => c.Meters)
            .HasForeignKey(m => m.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reading>()
            .HasOne(r => r.Meter)
            .WithMany(m => m.Readings)
            .HasForeignKey(r => r.MeterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bill>()
            .HasOne(b => b.Customer)
            .WithMany(c => c.Bills)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bill>()
            .HasOne(b => b.Reading)
            .WithOne(r => r.Bill)
            .HasForeignKey<Bill>(b => b.ReadingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Bill)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Service>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Services)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
