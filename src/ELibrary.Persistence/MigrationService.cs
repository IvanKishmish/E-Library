using ELibrary.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ELibrary.Persistence;

public static class MigrationService
{
    public static async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            var context = services.GetRequiredService<AppDbContext>();

            logger.LogInformation("Перевірка та застосування міграцій для PostgreSQL...");

            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }

            logger.LogInformation("Міграції успішно застосовані!");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Фатальна помилка під час накочування міграцій!");
            throw new InvalidOperationException("Database migration failed. Application cannot start.", ex);
        }
    }
}