using Microsoft.EntityFrameworkCore;
using Newproject.Models;

namespace Newproject.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.LastName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Phone)
                  .HasMaxLength(20);

            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasMany(e => e.Orders)
                  .WithOne(o => o.Customer)
                  .HasForeignKey(o => o.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Amount)
                  .IsRequired()
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasDefaultValue("Pending");

            entity.HasIndex(e => e.CustomerId);
        });
    }
}
