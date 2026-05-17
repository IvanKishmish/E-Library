using System.Text.Json;
using E_Library.Database;
using E_Library.Database.Entities;
using E_Library.Dtos.Authors;
using E_Library.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Microsoft.Extensions.Caching.Distributed;

namespace E_Library.Services;

public class AuthorService(AppDbContext db, IDistributedCache cache) : BaseService<Author, EntityId>(db), IAuthorService
{
    private const string AllKey = "all_authors";
    private string GetKey(EntityId id) => $"author_{id}";
    
    public new async Task<ErrorOr<IEnumerable<AuthorShortResponseDto>>> GetAllAsync()
    {
        var cached = await cache.GetStringAsync(AllKey);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<List<AuthorShortResponseDto>>(cached)!;
        
        var authors = await db.Authors
            .AsNoTracking()
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .Select(a => new AuthorShortResponseDto(
                a.Id,
                $"{a.FirstName} {a.LastName}".Trim()))
            .ToListAsync();

        await cache.SetStringAsync(AllKey, JsonSerializer.Serialize(authors), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        return authors;
    }

    public new async Task<ErrorOr<AuthorResponseDto?>> GetByIdAsync(EntityId id)
    {
        var key = GetKey(id);
        var cached = await cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<AuthorResponseDto>(cached)!;
        
        var author = await db.Authors
            .AsNoTracking()
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author is null)
            return Error.NotFound(
                code: "Author.NotFound",
                description: "Author with such id was not found.");
        
        var response = new AuthorResponseDto(
            author.Id, $"{author.FirstName} {author.LastName}".Trim(),
            author.Biography, author.Books.Select(b => b.Title));

        await cache.SetStringAsync(key, JsonSerializer.Serialize(response), 
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) });

        return response;
    }

    public async Task<ErrorOr<AuthorResponseDto>> CreateAsync(CreateAuthorRequestDto dto)
    {
        var names = dto.FullName.Split(' ', 2);
        var author = Author.Create(names[0], names.Length > 1 ? names[1] : "", dto.Biography);
        
        await base.AddAsync(author);
        await cache.RemoveAsync(AllKey);

        return new AuthorResponseDto(author.Id, dto.FullName, author.Biography, []);
    }

    public async Task<ErrorOr<Success>> UpdateAsync(EntityId id, UpdateAuthorRequestDto dto)
    {
        var author = await base.GetByIdAsync(id);

        if (author is null)
        {
            return Error.NotFound(
                code: "Author.NotFound",
                description: $"Impossible to update author: Author with id '{id}' was not found.");
        }
        
        var names = dto.FullName.Split(' ', 2);
        author.UpdateInfo(names[0], names.Length > 1 ? names[1] : "", dto.Biography);
        await base.UpdateAsync(author);

        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(GetKey(id));
        return Result.Success;
    }

    public new async Task<ErrorOr<Deleted>> DeleteAsync(EntityId id)
    {
        var author = await base.GetByIdAsync(id);
        if (author is null)
            return Error.NotFound(
                code: "Author.NotFound",
                description: $"Author with id '{id}' was not found.");

        await base.DeleteAsync(id);
        await cache.RemoveAsync(AllKey);
        await cache.RemoveAsync(GetKey(id));
        return Result.Deleted;
    }
}