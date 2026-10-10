using Microsoft.EntityFrameworkCore;
using UsersApi.Models;

namespace UsersApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Currency> Currencies => Set<Currency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Name).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();

            e.HasMany(u => u.Addresses)
             .WithOne(a => a.User)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Address>(e =>
        {
            e.Property(a => a.Street).IsRequired().HasMaxLength(200);
            e.Property(a => a.City).IsRequired().HasMaxLength(100);
            e.Property(a => a.Country).IsRequired().HasMaxLength(100);
            e.Property(a => a.ZipCode).HasMaxLength(20);
        });

        modelBuilder.Entity<Currency>(e =>
        {
            e.Property(c => c.Code).IsRequired().HasMaxLength(10);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(c => c.Code).IsUnique();
        });
    }
}