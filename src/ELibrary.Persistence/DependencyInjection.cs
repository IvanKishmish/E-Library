using ELibrary.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ELibrary.Persistence;

public static class DependencyInjection
{
   public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
   {
      // Налаштовуємо підключення до PostgreSQL
      services.AddDbContext<AppDbContext>(options =>
         options.UseNpgsql(configuration.GetConnectionString("DB_CONNECTION_STRING")));
      
      return services;
   } 
}