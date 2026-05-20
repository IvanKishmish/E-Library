using DotNetEnv;
using E_Library;
using E_Library.Database;
using E_Library.Services;
using E_Library.Services.Abstractions;
using E_Library.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

// Створюємо тимчасовий "Bootstrap" логер для відлову помилок старту
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext() // Дозволяє додавати контекстні дані до логів 
    .WriteTo.Console(theme: AnsiConsoleTheme.Code) // Гарна консоль 
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application...");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // Повна інтеграція Serilog у контейнер залежностей
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration) // Налаштування з appsettings.json 
        .ReadFrom.Services(services) // Дозволяє логувати стан сервісів 
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            theme: AnsiConsoleTheme.Code)
        .WriteTo.File(
            path: "logs/e-library-.txt",
            rollingInterval: RollingInterval.Day, // Ротація файлів щодня 
            retainedFileCountLimit: 7)); // Зберігати тільки останні 7 днів
                                         
    Env.Load();
    
    builder.Configuration.AddEnvironmentVariables();
    
    var connectionString = builder.Configuration.GetConnectionString("DB_CONNECTION_STRING");
    
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
    
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IAuthorService, AuthorService>();
    builder.Services.AddScoped<IBookService, BookService>();
    
    // Реєструємо Redis
    // Це замінює Redis в оперативній пам'яті. Всі методи IDistributedCache працюватимуть так само.
    builder.Services.AddDistributedMemoryCache();
    
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateAuthorValidator>();
    
    builder.Services.AddControllers();
    
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    var app = builder.Build();
    
    // Мінімізація ручного коду в контролерах через Middleware 
    // Тепер кожен HTTP-запит логується автоматично однією подією замість десятка шумних 
    app.UseSerilogRequestLogging(); 
    
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(); 
    }
    
    app.UseHttpsRedirection();
    
    await app.ApplyMigrationsAsync();
    
    app.MapControllers();
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush(); // Гарантує запис усіх залишків логів перед виходом 
}