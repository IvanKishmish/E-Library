using DotNetEnv;
using ELibrary.Application;
using ELibrary.Persistence;
using ELibrary.WebApi;
using Serilog;

Env.Load();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/ELibrary-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("=== Starting ELibrary Web API Application ===");

    var builder = WebApplication.CreateBuilder(args);

    // Передаємо керування логуванням до Serilog
    builder.Host.UseSerilog();

    // Реєстрація стандартних сервісів Web API
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(); 

    // Реєструємо розподілений кеш у пам'яті (IDistributedCache)
    builder.Services.AddDistributedMemoryCache();

    // Підключаємо шари архітектури через твої методи розширення
    builder.Services
        .AddApplication()
        .AddPersistence(builder.Configuration); 

    var app = builder.Build();

    // Налаштування конвеєра HTTP-запитів (Middleware)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Applying pending database migrations...");
    await app.ApplyMigrationsAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The application failed to start correctly!");
}
finally
{
    Log.CloseAndFlush(); // Гарантовано записуємо залишки логів у файл перед закриттям
}