using ELibrary.Application.Common.Interfaces;
using ELibrary.Domain.Entities;
using ELibrary.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ELibrary.Persistence.Repositories;

public class BookRepository(AppDbContext context) : IBookRepository
{
    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Books
            .Include(b => b.Author)
            .Include(b => b.Category)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public void Add(Book book) => context.Books.Add(book);

    public void Remove(Book book) => context.Books.Remove(book);
}