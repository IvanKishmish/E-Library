using E_Library.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Library.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    :  DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
}