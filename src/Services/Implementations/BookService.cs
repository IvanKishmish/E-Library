using System.Text.Json;
using E_Library.Database;
using E_Library.Database.Entities;
using E_Library.Dtos.Books;
using E_Library.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Microsoft.Extensions.Caching.Distributed;

namespace E_Library.Services;

public class BookService(AppDbContext db, IDistributedCache cache, ILogger<BookService> logger) 
    : BaseService<Book, EntityId>(db, logger), IBookService
{
    private const string AllKey = "all_books";
    private string GetKey(EntityId id) => $"book_{id}";
    
    public new async Task<ErrorOr<IEnumerable<BookResponseDto>>> GetAllAsync()
    {
        logger.LogInformation("Fetching all books from system");
        
        var cached = await cache.GetStringAsync(AllKey);
        if (!string.IsNullOrEmpty(cached))
        {
            logger.LogDebug("Books list retrieved from distributed cache");
            return JsonSerializer.Deserialize<List<BookResponseDto>>(cached)!;
        }
        
        var books = await db.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .OrderBy(b => b.Title)
            .Select(b => new BookResponseDto(
                b.Id, b.Title.Trim(), b.Description.Trim(), b.Price, 
                b.CategoryId, b.Category.Name, b.AuthorId))
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} books from database for caching", books.Count);
        
        await cache.SetStringAsync(AllKey, JsonSerializer.Serialize(books), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

        return books;
    }

    public new async Task<ErrorOr<BookResponseDto>> GetByIdAsync(EntityId id)
    {
        logger.LogDebug("Checking cache for book with ID {BookId}", id);
        
        var key = GetKey(id);
        var cached = await cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cached))
        {
            logger.LogDebug("Book {BookId} successfully found in cache", id);
            return JsonSerializer.Deserialize<BookResponseDto>(cached)!;
        }
        
        var book = await db.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book is null)
        {
            logger.LogWarning("Book business validation failed: ID {BookId} not found", id);
            return Error.NotFound(
                code: "Book.NotFound",
                description: $"Book with id '{id}' was not found.");
        }

        var response = new BookResponseDto(
            book.Id, book.Title.Trim(), book.Description.Trim(), book.Price, 
            book.CategoryId, book.Category.Name, book.AuthorId);

        await cache.SetStringAsync(key, JsonSerializer.Serialize(response), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) });

        return response;
    }

    public async Task<ErrorOr<BookResponseDto>> CreateAsync(CreateBookRequestDto dto)
    {
        try
        {
            logger.LogInformation("Initiating book creation workflow for: {BookTitle}", dto.Title);
            
            var category = await db.Categories.FindAsync(dto.CategoryId);
            if (category is null)
            {
                logger.LogWarning("Book creation failed: Category {CategoryId} does not exist", dto.CategoryId);
                return Error.NotFound(
                    code: "Category.NotFound",
                    description: "Неможливо створити книгу: вказаної категорії не існує.");
            }
            
            var authorExists = await db.Authors.AnyAsync(a => a.Id == dto.AuthorId);
            if (!authorExists)
            {
                logger.LogWarning("Book creation failed: Author {AuthorId} does not exist", dto.AuthorId);
                return Error.NotFound(
                    code: "Author.NotFound",
                    description: "Неможливо створити книгу: вказаного автора не існує.");
            }
            
            var book = Book.Create(
                dto.Title.Trim(), 
                dto.Description.Trim(), 
                dto.Price, 
                dto.CategoryId, 
                dto.AuthorId);
            
            await base.AddAsync(book);
            
            logger.LogInformation("Invalidating 'all books' cache due to new book creation");
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute business logic for creating book {BookTitle}", dto.Title);
            return Error.Failure("Book.CreateError", "An unexpected error occurred.");
        }
    }

    public async Task<ErrorOr<Success>> UpdateAsync(EntityId id, UpdateBookRequestDto dto)
    {
        logger.LogInformation("Requested update for book {BookId} with new title {NewTitle}", id, dto.Title);
        
        var book = await base.GetByIdAsync(id);
        if (book is null)
        {
            logger.LogWarning("Update rejected: Book {BookId} does not exist", id);
            return Error.NotFound(
                code: "Book.NotFound", 
                description: $"Книгу з ID '{id}' не знайдено.");
        }

        if (book.CategoryId != dto.CategoryId)
        {
            logger.LogDebug("Book {BookId} category changed. Verifying new category {CategoryId}", id, dto.CategoryId);
            
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                logger.LogWarning("Update rejected: New category {CategoryId} does not exist", dto.CategoryId);
                return Error.NotFound(
                    code: "Category.NotFound", 
                    description: "Неможливо оновити: вказаної нової категорії не існує.");
            }
        }
        
        book.UpdateInfo(
            dto.Title.Trim(), 
            dto.Description.Trim(), 
            dto.Price, 
            dto.CategoryId);
        
        await base.UpdateAsync(book);
        
        logger.LogDebug("Clearing caches for updated book {BookId}", id);
        
        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(GetKey(id));
        
        return Result.Success;
    }

    public new async Task<ErrorOr<Deleted>> DeleteAsync(EntityId id)
    {
        logger.LogInformation("Processing business request to delete book {BookId}", id);
        
        var book = await base.GetByIdAsync(id);
        if (book is null)
        {
            logger.LogWarning("Delete canceled: Book {BookId} is missing", id);
            return Error.NotFound("Book.NotFound", "Book not found or already deleted.");
        }

        await base.DeleteAsync(id);
        
        logger.LogDebug("Clearing caches for deleted book {BookId}", id);
        
        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(GetKey(id));
        
        return Result.Deleted;
    }
    
    public async Task<ErrorOr<IEnumerable<BookResponseDto>>> GetFilteredAsync(BookFilterParameters filter)
    {
        logger.LogInformation("Applying filters for query searching. SearchTerm: {SearchTerm}, CategoryId: {CategoryId}", 
            filter.SearchTerm, filter.CategoryId);
        
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
        
        logger.LogInformation("Filtered query execution finished. Found {Count} matching books", books.Count);

        return books;
    }
}