using System.Text.Json;
using E_Library.Database;
using E_Library.Database.Entities;
using E_Library.Dtos.Books;
using E_Library.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Microsoft.Extensions.Caching.Distributed;

namespace E_Library.Services;

public class BookService(AppDbContext db, IDistributedCache cache) : BaseService<Book, EntityId>(db), IBookService
{
    private const string AllKey = "all_books";
    private string GetKey(EntityId id) => $"book_{id}";
    
    public new async Task<ErrorOr<IEnumerable<BookResponseDto>>> GetAllAsync()
    {
        var cached = await cache.GetStringAsync(AllKey);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<List<BookResponseDto>>(cached)!;
        
        var books = await db.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .OrderBy(b => b.Title)
            .Select(b => new BookResponseDto(
                b.Id, b.Title.Trim(), b.Description.Trim(), b.Price, 
                b.CategoryId, b.Category.Name, b.AuthorId))
            .ToListAsync();

        await cache.SetStringAsync(AllKey, JsonSerializer.Serialize(books), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

        return books;
    }

    public new async Task<ErrorOr<BookResponseDto>> GetByIdAsync(EntityId id)
    {
        var key = GetKey(id);
        var cached = await cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<BookResponseDto>(cached)!;
        
        var book = await db.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null)
            return Error.NotFound(
                code: "Book.NotFound",
                description: $"Book with id '{id}' was not found.");

        var response = new BookResponseDto(
            book.Id, book.Title.Trim(), book.Description.Trim(), book.Price, 
            book.CategoryId, book.Category.Name, book.AuthorId);

        await cache.SetStringAsync(key, JsonSerializer.Serialize(response), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) });

        return response;
    }

    public async Task<ErrorOr<BookResponseDto>> CreateAsync(CreateBookRequestDto dto)
    {
        var category = await db.Categories.FindAsync(dto.CategoryId);
        if (category is null)
            return Error.NotFound(
                code: "Category.NotFound",
                description: "Неможливо створити книгу: вказаної категорії не існує.");

        var authorExists = await db.Authors.AnyAsync(a => a.Id == dto.AuthorId);
        if (!authorExists)
            return Error.NotFound(
                code: "Author.NotFound",
                description: "Неможливо створити книгу: вказаного автора не існує.");

        var book = Book.Create(
            dto.Title.Trim(), 
            dto.Description.Trim(), 
            dto.Price, 
            dto.CategoryId, 
            dto.AuthorId);
    
        await base.AddAsync(book);
    
        await cache.RemoveAsync(AllKey);
        
        return new BookResponseDto(
            book.Id, 
            book.Title, 
            book.Description, 
            book.Price, 
            book.CategoryId, 
            category.Name, 
            book.AuthorId);
    }

    public async Task<ErrorOr<Success>> UpdateAsync(EntityId id, UpdateBookRequestDto dto)
    {
        var book = await base.GetByIdAsync(id);
        if (book is null)
            return Error.NotFound(
                code: "Book.NotFound", 
                description: $"Книгу з ID '{id}' не знайдено.");

        if (book.CategoryId != dto.CategoryId)
        {
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
                return Error.NotFound(
                    code: "Category.NotFound", 
                    description: "Неможливо оновити: вказаної нової категорії не існує.");
        }
        
        book.UpdateInfo(
            dto.Title.Trim(), 
            dto.Description.Trim(), 
            dto.Price, 
            dto.CategoryId);
        
        await base.UpdateAsync(book);

        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(GetKey(id));
        
        return Result.Success;
    }

    public new async Task<ErrorOr<Deleted>> DeleteAsync(EntityId id)
    {
        var book = await base.GetByIdAsync(id);
        if (book is null)
            return Error.NotFound("Book.NotFound", "Book not found or already deleted.");

        await base.DeleteAsync(id);
        
        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(GetKey(id));
        
        return Result.Deleted;
    }
    
    public async Task<ErrorOr<IEnumerable<BookResponseDto>>> GetFilteredAsync(BookFilterParameters filter)
    {
        // Починаємо будувати запит (IQueryable не йде в базу одразу)
        var query = db.Books.AsNoTracking().AsQueryable();

        // Пошук за назвою 
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(term));
        }

        // Фільтр за ціною
        if (filter.MinPrice.HasValue) 
            query = query.Where(b => b.Price >= filter.MinPrice.Value);
    
        if (filter.MaxPrice.HasValue) 
            query = query.Where(b => b.Price <= filter.MaxPrice.Value);

        // Фільтр за категорією
        if (filter.CategoryId.HasValue)
            query = query.Where(b => b.CategoryId == filter.CategoryId.Value);
        
        var books = await query
            .Include(b => b.Category)
            .OrderBy(b => b.Title) 
            .Select(b => new BookResponseDto(
                b.Id, b.Title.Trim(), b.Description.Trim(), b.Price, 
                b.CategoryId, b.Category.Name, b.AuthorId))
            .ToListAsync();

        return books;
    }
}