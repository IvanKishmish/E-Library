using E_Library.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Library.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    :  DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Це автоматично знайде всі класи, що реалізують IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ChangeTracker — це механізм EF, який бачить усі зміни в об'єктах
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IAuditable && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (IAuditable)entityEntry.Entity;
            var now = DateTimeOffset.UtcNow; // Використовуємо UTC — це стандарт

            if (entityEntry.State == EntityState.Added)
            {
                // Використовуємо Reflection або Cast, щоб встановити значення
                entityEntry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = now;
            }
            else
            {
                entityEntry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}