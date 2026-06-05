using ELibrary.Domain.Common;
using ELibrary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Persistence.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Одразу фільтруємо сутності, які реалізують IAuditable
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IAuditable && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        var now = DateTimeOffset.UtcNow;

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = now;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}