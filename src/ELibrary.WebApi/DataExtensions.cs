using ELibrary.Persistence;

namespace ELibrary.WebApi;

public static class DataExtensions
{
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        await MigrationService.ApplyMigrationsAsync(scope.ServiceProvider);
    }
}