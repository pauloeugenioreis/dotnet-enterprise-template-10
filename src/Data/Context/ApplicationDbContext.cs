using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Domain.Entities;

namespace ProjectTemplate.Data.Context;

/// <summary>
/// Entity Framework Core Database Context
/// This is the default ORM implementation
/// To use another ORM, create an alternative context implementation
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    // DbSets
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // Add your DbSets here
    // Example: public DbSet<YourEntity> YourEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure your entities here
        // Apply configurations from assemblies
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters example
        // modelBuilder.Entity<EntityBase>().HasQueryFilter(e => e.IsActive);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set UpdatedAt for modified entities
        var entries = ChangeTracker.Entries<EntityBase>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
